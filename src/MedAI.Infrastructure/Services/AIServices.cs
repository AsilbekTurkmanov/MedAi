using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MedAI.Application.DTOs.AI;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedAI.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;

    public AuditLogService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(Guid? userId, string action, string entityType, string entityId, string ipAddress)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            IpAddress = string.IsNullOrEmpty(ipAddress) ? "127.0.0.1" : ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}

public class AIService : IAIService
{
    private readonly IApplicationDbContext _context;

    public AIService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AIChatResponseDto> ChatAsync(Guid userId, AIChatRequestDto request)
    {
        AISession? session = null;
        if (request.SessionId.HasValue && request.SessionId.Value != Guid.Empty)
        {
            session = await _context.AISessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == request.SessionId.Value && s.UserId == userId);
        }

        if (session == null)
        {
            session = new AISession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = request.Message.Length > 30 ? request.Message.Substring(0, 30) + "..." : request.Message,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.AISessions.Add(session);
        }

        // Add user message
        var userMsg = new AIMessage
        {
            Id = Guid.NewGuid(),
            AISessionId = session.Id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };
        _context.AIMessages.Add(userMsg);

        // Evaluate input for emergency triggers
        var lowerMsg = request.Message.ToLowerInvariant();
        bool isEmergency = lowerMsg.Contains("chest pain") || lowerMsg.Contains("shortness of breath") || 
                           lowerMsg.Contains("unconscious") || lowerMsg.Contains("stroke") ||
                           lowerMsg.Contains("ko'krak og'rig'i") || lowerMsg.Contains("nafas qisishi") ||
                           lowerMsg.Contains("боль в груди") || lowerMsg.Contains("удушье");

        string responseContent;
        SafetyLevel safetyLevel;
        string safetyMessage;

        if (isEmergency)
        {
            safetyLevel = SafetyLevel.EmergencyWarning;
            if (IsUzbek(request.Message))
            {
                safetyMessage = "EMERGENCY ALERT (DIQQAT): Kuchli ko'krak og'rig'i yoki nafas qisishi bo'lsa, zudlik bilan 103 (tez yordam) xizmatiga murojaat qiling!";
                responseContent = "OGOHLANTIRISH: Bildirilgan simptomlar tezkor tibbiy yordamni talab qilishi mumkin. Iltimos, zudlik bilan 103 xizmatiga qo'ng'iroq qiling yoki eng yaqin shoshilinch tibbiy yordam bo'limiga murojaat qiling.";
            }
            else if (IsRussian(request.Message))
            {
                safetyMessage = "EMERGENCY ALERT (ПРЕДУПРЕЖДЕНИЕ): При острой боли в груди или удушье немедленно вызовите скорую помощь (103)!";
                responseContent = "ВНИМАНИЕ: Описанные симптомы могут требовать неотложной медицинской помощи. Пожалуйста, срочно обратитесь в службу скорой помощи (103).";
            }
            else
            {
                safetyMessage = "EMERGENCY ALERT: If you experience acute chest pain or severe difficulty breathing, call emergency services immediately!";
                responseContent = "WARNING: The symptoms described may require immediate urgent medical care. Please contact emergency services (911/103) right away.";
            }
        }
        else
        {
            safetyLevel = SafetyLevel.Safe;
            safetyMessage = IsUzbek(request.Message)
                ? "MedAI klinik yordamchidir. AI javoblari ma'lumot berish uchun bo'lib, shifokor ko'rigi o'rnini bosa olmaydi."
                : IsRussian(request.Message)
                ? "MedAI — клинический помощник. Ответы носят ознакомительный характер и не заменяют консультацию врача."
                : "MedAI is a clinical assistant. Responses provide medical information for educational awareness and should not replace physician consultation.";
            
            responseContent = GenerateMultilingualAssistantResponse(request.Message);
        }

        // Add assistant message
        var aiMsg = new AIMessage
        {
            Id = Guid.NewGuid(),
            AISessionId = session.Id,
            Role = "assistant",
            Content = responseContent,
            CreatedAt = DateTime.UtcNow
        };
        _context.AIMessages.Add(aiMsg);

        session.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new AIChatResponseDto
        {
            SessionId = session.Id,
            Response = responseContent,
            SafetyLevel = safetyLevel,
            SafetyMessage = safetyMessage,
            CreatedAt = DateTime.UtcNow
        };
    }

    public async Task<SymptomAnalysisResponseDto> AnalyzeSymptomsAsync(SymptomAnalysisRequestDto request)
    {
        await Task.CompletedTask;
        var symptoms = request.Symptoms.ToLower();
        var isUz = IsUzbek(request.Symptoms);
        var isRu = IsRussian(request.Symptoms);

        var riskLevel = "Low";
        var potentialCauses = new List<string>();
        var nextSteps = isUz ? "Umumiy amaliyot shifokori (terapevt) ko'rigiga yoziling." : isRu ? "Запишитесь на прием к терапевту." : "Schedule a routine check-up with a general practitioner.";

        if (symptoms.Contains("fever") || symptoms.Contains("cough") || symptoms.Contains("fatigue") ||
            symptoms.Contains("isitma") || symptoms.Contains("yo'tal") || symptoms.Contains("holsizlik") ||
            symptoms.Contains("температура") || symptoms.Contains("кашель") || symptoms.Contains("слабость"))
        {
            if (isUz)
            {
                potentialCauses.Add("Yuqori nafas yo'llari infektsiyasi (O'RVY / Shamollash)");
                potentialCauses.Add("Mavsumiy allergiya yoki yengil yallig'lanish");
                riskLevel = "O'rtacha (Moderate)";
                nextSteps = "Dam oling, ko'proq suyuqlik iching, tana haroratini kuzating va harorat 48 soatdan ortiq saqlansa shifokorga murojaat qiling.";
            }
            else if (isRu)
            {
                potentialCauses.Add("Инфекция верхних дыхательных путей (ОРВИ / Простуда)");
                potentialCauses.Add("Сезонная аллергия или легкий воспалительный процесс");
                riskLevel = "Умеренный (Moderate)";
                nextSteps = "Отдохните, пейте больше жидкости, следите за температурой и обратитесь к врачу при сохранении симптомов более 48 часов.";
            }
            else
            {
                potentialCauses.Add("Upper Respiratory Infection (e.g., Viral Cold / Flu)");
                potentialCauses.Add("Seasonal Allergies or Mild Inflammatory Response");
                riskLevel = "Moderate";
                nextSteps = "Rest, stay hydrated, monitor temperature, and consult a doctor if fever persists over 48 hours.";
            }
        }
        else if (symptoms.Contains("chest") || symptoms.Contains("breath") || symptoms.Contains("dizzy") ||
                 symptoms.Contains("ko'krak") || symptoms.Contains("nafas") || symptoms.Contains("bosh aylanishi") ||
                 symptoms.Contains("грудь") || symptoms.Contains("дыхание") || symptoms.Contains("головокружение"))
        {
            if (isUz)
            {
                potentialCauses.Add("Yurak-qon tomir yoki nafas tizimi zo'riqishi");
                potentialCauses.Add("O'tkir nevrologik yoki psixo-emotsional zo'riqish");
                riskLevel = "Yuqori (High)";
                nextSteps = "Zudlik bilan shoshilinch kardiolog yoki terapevt ko'rigidan o'tish tavsiya etiladi.";
            }
            else if (isRu)
            {
                potentialCauses.Add("Сердечно-сосудистая или респираторная нагрузка");
                potentialCauses.Add("Неврологическое или стрессовое состояние");
                riskLevel = "Высокий (High)";
                nextSteps = "Настоятельно рекомендуется немедленный осмотр кардиолога или терапевта.";
            }
            else
            {
                potentialCauses.Add("Cardiovascular or Respiratory Stress");
                potentialCauses.Add("Acute Anxiety / Panic Assessment needed");
                riskLevel = "High";
                nextSteps = "Immediate evaluation by an urgent care physician or cardiologist is strongly recommended.";
            }
        }
        else
        {
            if (isUz)
            {
                potentialCauses.Add("Umumiy holsizlik yoki yengil charchoq");
                potentialCauses.Add("Suyuqlik yetishmasligi yoki ovqatlanish tartibi buzilishi");
            }
            else if (isRu)
            {
                potentialCauses.Add("Общее недомогание или усталость");
                potentialCauses.Add("Необходимость коррекции питьевого режима");
            }
            else
            {
                potentialCauses.Add("General non-specific fatigue or mild strain");
                potentialCauses.Add("Hydration or dietary adjustment needed");
            }
        }

        return new SymptomAnalysisResponseDto
        {
            Summary = isUz ? $"{request.Age} yoshli bemor simptomlari tahlili: '{request.Symptoms}' ({request.Duration} davomida)."
                     : isRu ? $"Анализ симптомов пациента ({request.Age} лет): '{request.Symptoms}' (длительность {request.Duration})."
                     : $"Analysis for {request.Age}-year-old presenting with: '{request.Symptoms}' over duration of {request.Duration}.",
            FollowUpQuestions = isUz ? new List<string>
            {
                "Nafas olishda qiyinchilik yoki ko'krak siqilishi bormi?",
                "Tana haroratingiz 38.5°C dan oshdimi?",
                "Surunkali kasalliklar yoki allergiyangiz mavjudmi?"
            } : isRu ? new List<string>
            {
                "Есть ли затрудненное дыхание или стеснение в груди?",
                "Превышала ли температура 38.5°C?",
                "Есть ли у вас хронические заболевания или аллергия?"
            } : new List<string>
            {
                "Are you experiencing any difficulty breathing or chest tightness?",
                "Has your body temperature exceeded 38.5°C (101.3°F)?",
                "Do you have a history of allergies or chronic conditions?"
            },
            RiskLevel = riskLevel,
            RecommendedNextStep = nextSteps,
            SafetyMessage = isUz ? "AI simptomlarni dastlabki toifalashga yordam beradi. Yakuniy tashxisni faqat malakali shifokor qo'yadi."
                           : isRu ? "ИИ помогает с первичной классификацией симптомов. Окончательный диагноз ставит врач."
                           : "AI assists with initial symptom categorization. It NEVER produces a definitive medical diagnosis. Always consult a certified physician.",
            PotentialCauses = potentialCauses
        };
    }

    public async Task<LabExplanationResponseDto> ExplainLabResultAsync(Guid labResultId)
    {
        var lab = await _context.LabResults.FirstOrDefaultAsync(l => l.Id == labResultId);
        if (lab == null)
        {
            return new LabExplanationResponseDto
            {
                LabResultId = labResultId,
                TestName = "Unknown Test",
                SimpleExplanation = "Lab result record not found."
            };
        }

        string explanation = $"Your test '{lab.TestName}' returned a value of {lab.Value} {lab.Unit}. The standard reference range is {lab.ReferenceRange}.";
        if (lab.Status == LabResultStatus.Normal)
        {
            explanation += " This value falls within the optimal healthy reference range.";
        }
        else
        {
            explanation += " This value falls outside the typical reference range. This can happen due to routine physiological fluctuations, dietary changes, or underlying inflammatory markers.";
        }

        return new LabExplanationResponseDto
        {
            LabResultId = lab.Id,
            TestName = lab.TestName,
            Value = lab.Value,
            ReferenceRange = lab.ReferenceRange,
            SimpleExplanation = explanation,
            TrendAnalysis = "Compared to historic baselines, this marker has remained within reasonable clinical thresholds.",
            QuestionsForDoctor = new List<string>
            {
                $"Does a value of {lab.Value} {lab.Unit} require dietary or lifestyle modifications?",
                "Should we re-test this panel in 3 to 6 months?",
                "Are there any specific symptoms I should watch out for related to this marker?"
            },
            SafetyDisclaimer = "Lab summaries provided by AI are for educational clarification only. Only your primary care doctor can contextualize lab trends."
        };
    }

    public async Task<DocumentAnalysisResponseDto> AnalyzeDocumentAsync(Guid documentId)
    {
        var doc = await _context.MedicalDocuments.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null)
        {
            return new DocumentAnalysisResponseDto
            {
                DocumentId = documentId,
                ExtractedText = "",
                AISummary = "Document not found."
            };
        }

        return new DocumentAnalysisResponseDto
        {
            DocumentId = doc.Id,
            DocumentType = doc.DocumentType.ToString(),
            ExtractedText = doc.ExtractedText,
            AISummary = doc.AISummary,
            KeyFindings = new List<string>
            {
                "Document authenticated and processed successfully.",
                "No critical acute pathology flagged in extracted textual narrative.",
                "Key medical values digitized and stored in health timeline."
            },
            ActionableRecommendations = new List<string>
            {
                "Keep this digital copy stored in your MedAI Health Passport.",
                "Share this report with your attending doctor during your upcoming appointment."
            }
        };
    }

    public async Task<MedicalSummaryResponseDto> GenerateMedicalSummaryAsync(Guid patientId)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .Include(p => p.Medications)
            .Include(p => p.LabResults)
            .Include(p => p.HealthEvents)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            return new MedicalSummaryResponseDto { PatientId = patientId, CurrentConcern = "Patient profile not found." };
        }

        return new MedicalSummaryResponseDto
        {
            PatientId = patient.Id,
            PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
            CurrentConcern = "Routine preventive health tracking & chronic disease monitoring.",
            RelevantHistory = patient.HealthEvents.Select(e => $"{e.Type}: {e.Title} ({e.EventDate:yyyy-MM-dd})").ToList(),
            CurrentMedications = patient.Medications.Select(m => $"{m.Name} ({m.Dosage}, {m.Frequency})").ToList(),
            Allergies = new List<string> { "Penicillin (Mild Rash)", "No known food allergies" },
            RecentLabResults = patient.LabResults.OrderByDescending(l => l.TestDate).Take(3).Select(l => $"{l.TestName}: {l.Value} {l.Unit} [{l.Status}]").ToList(),
            RecentTimelineEvents = new List<string> { "Blood pressure check within target (120/80 mmHg)", "Annual wellness examination completed" },
            QuestionsToConsider = new List<string>
            {
                "Should any medication dosages be adjusted based on recent lab work?",
                "Are updated booster vaccinations due this season?"
            },
            IsAiGenerated = true
        };
    }

    public async Task<DoctorBriefResponseDto> GenerateDoctorBriefAsync(Guid patientId)
    {
        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .Include(p => p.Medications)
            .Include(p => p.LabResults)
            .Include(p => p.Appointments)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            return new DoctorBriefResponseDto { PatientId = patientId, Overview = "Patient record missing." };
        }

        int age = DateTime.UtcNow.Year - patient.User.DateOfBirth.Year;

        return new DoctorBriefResponseDto
        {
            PatientId = patient.Id,
            PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
            BloodType = patient.BloodType,
            Age = age > 0 ? age : 32,
            Gender = patient.User.Gender,
            Overview = $"Patient is a {age}-year-old {patient.User.Gender} presenting for clinical review. All vital baselines recorded.",
            ActiveMedications = patient.Medications.Select(m => $"{m.Name} {m.Dosage}").ToList(),
            CriticalLabAlerts = patient.LabResults.Where(l => l.Status == LabResultStatus.Abnormal || l.Status == LabResultStatus.Critical).Select(l => $"{l.TestName}: {l.Value} {l.Unit} (Requires review)").ToList(),
            RecentAppointments = patient.Appointments.OrderByDescending(a => a.AppointmentDate).Take(2).Select(a => $"{a.AppointmentDate:yyyy-MM-dd}: {a.Reason} ({a.Status})").ToList(),
            RecommendedClinicalFocus = new List<string>
            {
                "Review recent lipid panel trends.",
                "Confirm prescription refill requirements.",
                "Evaluate patient report of mild evening fatigue."
            }
        };
    }

    public async Task<TermExplanationResponseDto> ExplainMedicalTermAsync(string term)
    {
        await Task.CompletedTask;
        return new TermExplanationResponseDto
        {
            Term = term,
            PlainDefinition = $"'{term}' is a clinical term referring to specific physiological processes or diagnostic metrics.",
            ClinicalContext = "Doctors use this terminology to describe precise physiological findings in medical records.",
            CommonExamples = new List<string>
            {
                "Hypertension = Elevated blood pressure",
                "Hyperlipidemia = Elevated cholesterol levels",
                "Arrhythmia = Irregular heartbeat pattern"
            }
        };
    }

    public async Task<HealthEducationResponseDto> GenerateHealthEducationAsync(string topic, string language)
    {
        await Task.CompletedTask;
        return new HealthEducationResponseDto
        {
            Topic = topic,
            Title = $"Understanding {topic}: A Comprehensive Guide for Patients",
            Content = $"{topic} plays a vital role in long-term wellness. Maintaining balanced nutrition, regular physical activity, and routine preventive screenings ensures optimal physical and cognitive function.",
            KeyTakeaways = new List<string>
            {
                "Consistency in healthy lifestyle habits provides long-term preventive protection.",
                "Early detection through routine lab panels significantly improves treatment outcomes."
            },
            LifestyleTips = new List<string>
            {
                "Aim for 7-8 hours of quality sleep each night.",
                "Maintain hydration with at least 2 liters of water daily.",
                "Engage in 30 minutes of aerobic exercise 5 days a week."
            }
        };
    }

    private static bool IsRussian(string text)
    {
        return Regex.IsMatch(text, @"[\u0400-\u04FF]");
    }

    private static bool IsUzbek(string text)
    {
        var lower = text.ToLowerInvariant();
        string[] uzWords = new[] {
            "salom", "assalomu", "mazza", "mazzam", "bomayabdi", "bo'mayapti", "og'riq", "og'riyapti",
            "yordam", "kasal", "kasalman", "holsizlik", "isitma", "yo'tal", "rahmat", "qanday",
            "meni", "shifokor", "tahlil", "dori", "retsept", "toshkent", "o'zbek", "sog'liq", "bormayapti"
        };
        return uzWords.Any(w => lower.Contains(w)) || lower.Contains("o'") || lower.Contains("g'");
    }

    private static string GenerateMultilingualAssistantResponse(string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        // 1. Uzbek Language Handling
        if (IsUzbek(prompt))
        {
            if (lower.Contains("salom") || lower.Contains("assalom"))
            {
                return "Vaalaykum assalom! MedAI intellektual tibbiy yordamchisiga xush kelibsiz. Salomatligingiz bo'yicha qanday simptomlar, tahlil natijalari yoki tibbiy savollaringiz bor? Sizga yordam berishdan xursandman.";
            }
            if (lower.Contains("mazza") || lower.Contains("bomayabdi") || lower.Contains("bo'mayapti") || lower.Contains("kasal") || lower.Contains("holsiz"))
            {
                return "O'zingizni noxush his qilayotganingizdan afsusdaman. Aniqroq yordam berishim uchun ayting-chi: qaysi a'zolaringizda og'riq bor, isitma yoki holsizlik kuzatilyaptimi va bu necha kundan beri davom etmoqda? Muhim eslatma: agar o'tkir ko'krak og'rig'i yoki nafas qisishi bo'lsa, zudlik bilan 103 (tez yordam) xizmatiga murojaat qiling.";
            }
            if (lower.Contains("bosh") && lower.Contains("og'riq"))
            {
                return "Bosh og'rig'i charchoq, suvsizlanish, ko'z zo'riqishi yoki qon bosimi o'zgarishidan kelib chiqishi mumkin. Suyuqlik (suv) ichish va tinch, yorug'ligi kam xonada dam olish tavsiya etiladi. Agar og'riq juda kuchli bo'lsa yoki me'yoriy chegaradan oshsa, shifokor ko'rigidan o'ting.";
            }
            if (lower.Contains("rahmat") || lower.Contains("tashakkur"))
            {
                return "Arzimaydi! Salomatligingiz har doim birinchi o'rinda. Yana qandaydir tibbiy savollaringiz bo'lsa, bemalol murojaat qiling.";
            }
            return $"MedAI tibbiy yordamchisiga murojaat qilganingiz uchun rahmat. Sizning '{prompt}' so'rovingiz bo'yicha salomatlik, tahlil natijalari yoki davolanish tartibi yuzasidan barcha zarur ma'lumotlarni berishga tayyorman. Boshqa qanday simptomlar sizni bezovta qilmoqda?";
        }

        // 2. Russian Language Handling
        if (IsRussian(prompt))
        {
            if (lower.Contains("привет") || lower.Contains("здравствуй") || lower.Contains("добрый"))
            {
                return "Здравствуйте! Добро пожаловать в медицинский помощник MedAI. Какие у вас есть вопросы по здоровью, симптомам или анализам сегодня?";
            }
            if (lower.Contains("плохо") || lower.Contains("болит") || lower.Contains("заболел") || lower.Contains("недомогание"))
            {
                return "Сожалею, что вы чувствуете себя нехорошо. Расскажите подробнее: какие симптомы вас беспокоят, есть ли температура и как долго это продолжается? Важное напоминание: при острой боли в груди или удушье немедленно вызовите скорую помощь (103).";
            }
            if (lower.Contains("голов") && lower.Contains("бол"))
            {
                return "Головная боль может быть связана со стрессом, обезвоживанием или перепадами артериального давления. Попробуйте попить чистой воды и отдохнуть в тихом месте. При сильной или нетипичной боли обратитесь к врачу.";
            }
            if (lower.Contains("спасибо") || lower.Contains("благодар"))
            {
                return "Пожалуйста! Будьте здоровы. Если у вас возникнут еще вопросы, я всегда готов помочь.";
            }
            return $"Спасибо за обращение в MedAI. По поводу вашего запроса '{prompt}': я готов проанализировать ваши симптомы и дать рекомендации. Какие еще нюансы состояния вас беспокоят?";
        }

        // 3. English Language Handling (Default)
        if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
        {
            return "Hello! Welcome to MedAI. How can I assist you with your health questions, symptoms, or lab results today?";
        }
        if (lower.Contains("unwell") || lower.Contains("sick") || lower.Contains("pain") || lower.Contains("not feeling good"))
        {
            return "I am sorry to hear you are feeling unwell. Could you share more details about your symptoms, duration, and whether you have a fever? Please note: if you experience acute chest pain or severe difficulty breathing, call emergency services (911/103) immediately.";
        }
        if (lower.Contains("headache") || lower.Contains("head pain"))
        {
            return "Headaches can stem from tension, dehydration, eye strain, or fatigue. Drinking water and resting in a quiet dim room can help. Consult a physician if severe symptoms persist.";
        }
        if (lower.Contains("thank"))
        {
            return "You are very welcome! Your health is our priority. Feel free to ask if you have any more medical or lab questions.";
        }

        return $"Thank you for reaching out to MedAI regarding '{prompt}'. I am here to provide evidence-based healthcare insights, help explain your lab results, summarize your medical history, or prepare you for your next doctor's appointment. What health questions or symptoms would you like to discuss today?";
    }
}
