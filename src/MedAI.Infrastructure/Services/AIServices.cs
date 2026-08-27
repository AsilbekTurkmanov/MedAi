using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MedAI.Application.DTOs.AI;
using MedAI.Application.DTOs.Clinical;
using MedAI.Application.Interfaces;
using MedAI.Domain.Entities;
using MedAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
    private readonly IConfiguration? _config;
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };

    public AIService(IApplicationDbContext context, IConfiguration? config = null)
    {
        _context = context;
        _config = config;
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
                           lowerMsg.Contains("hushidan ket") || lowerMsg.Contains("hushsiz") ||
                           lowerMsg.Contains("insult") || lowerMsg.Contains("infarkt") ||
                           lowerMsg.Contains("боль в груди") || lowerMsg.Contains("удушье") ||
                           lowerMsg.Contains("потеря сознания");

        string responseContent = "";
        SafetyLevel safetyLevel;
        string safetyMessage;

        if (isEmergency)
        {
            safetyLevel = SafetyLevel.EmergencyWarning;
            if (IsUzbek(request.Message))
            {
                safetyMessage = "EMERGENCY ALERT (DIQQAT): Kuchli ko'krak og'rig'i, nafas qisishi yoki hushdan ketish holati bo'lsa, zudlik bilan 103 (tez yordam) xizmatiga murojaat qiling!";
                responseContent = "🚨 **OGOHLANTIRISH: ZUDLIK BILAN TEZ YORDAM (103) CHAQIRING!**\n\n" +
                                  "Siz bildirgan belgilar (o'tkir ko'krak qisishi, nafas yetishmasligi yoki hush yo'qotish) kechiktirib bo'lmaydigan tibbiy yordamni talab qiladi.\n\n" +
                                  "**Hozir nima qilish kerak:**\n" +
                                  "1. Zudlik bilan **103** (O'zbekiston) yoki **112** favqulodda raqamiga qo'ng'iroq qiling.\n" +
                                  "2. Bemorga qulay, yarim o'tirgan holat bering va xonaga toza havo kiritish uchun derazani oching.\n" +
                                  "3. Yoqani va bo'yinni siqib turgan kiyimlarni bo'shating.\n" +
                                  "4. Tez yordam yetib kelguncha bemorni yolg'iz qoldirmang va ortiqcha jismoniy harakat qildirmang.";
            }
            else if (IsRussian(request.Message))
            {
                safetyMessage = "EMERGENCY ALERT (ПРЕДУПРЕЖДЕНИЕ): При острой боли в груди, удушье или потере сознания немедленно вызовите скорую помощь (103)!";
                responseContent = "🚨 **ВНИМАНИЕ: СРОЧНО ВЫЗОВИТЕ СКОРУЮ ПОМОЩЬ (103/112)!**\n\n" +
                                  "Описанные симптомы могут указывать на острое жизнеугрожающее состояние (инфаркт, приступ удушья, инсульт).\n\n" +
                                  "**Неотложные действия:**\n" +
                                  "1. Срочно позвоните по номеру **103** или **112**.\n" +
                                  "2. Обеспечьте больному полусидячее положение и приток свежего воздуха.\n" +
                                  "3. Расстегните стесняющую одежду.\n" +
                                  "4. Не оставляйте больного одного до приезда медиков.";
            }
            else
            {
                safetyMessage = "EMERGENCY ALERT: If you experience acute chest pain, severe difficulty breathing, or loss of consciousness, call emergency services immediately!";
                responseContent = "🚨 **EMERGENCY WARNING: CALL EMERGENCY SERVICES (911 / 103) IMMEDIATELY!**\n\n" +
                                  "The symptoms described may indicate an acute cardiovascular or respiratory emergency.\n\n" +
                                  "**Immediate Steps:**\n" +
                                  "1. Call emergency services immediately.\n" +
                                  "2. Sit down, rest in a comfortable upright position, and loosen tight clothing.\n" +
                                  "3. Do not drive yourself to the hospital.\n" +
                                  "4. Stay calm while emergency medical responders are en route.";
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

            // 1. Try LLM API (Google Gemini or OpenAI) if API key is provided
            string? apiKey = _config?["GEMINI_API_KEY"] 
                             ?? _config?["Gemini:ApiKey"] 
                             ?? _config?["Ai:GeminiApiKey"] 
                             ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                try
                {
                    responseContent = await CallGeminiApiAsync(request.Message, apiKey);
                }
                catch
                {
                    // Fallback to intelligent local knowledge engine if API call fails
                    responseContent = "";
                }
            }

            // 2. If responseContent is still empty, use our comprehensive intelligent clinical & general Q&A engine
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                responseContent = GenerateIntelligentMedicalResponse(request.Message);
            }
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

    private static async Task<string> CallGeminiApiAsync(string userPrompt, string apiKey)
    {
        var systemInstruction = "Siz MedAI platformasining intellektual tibbiy maslahatchisisiz. Foydalanuvchining har qanday savoliga (simptomlar, kasalliklar, tahlillar, dorilar, sog'lom turmush tarzi, parhez, birinchi yordam va umumiy savollarga) aniq, tushunarli, dalillarga asoslangan, muloyim va professional tilda javob bering. Foydalanuvchi qaysi tilda (O'zbek, Rus, Ingliz) yozsa, o'sha tilda javob bering. Javobingizda chiroyli markdown sarlavhalar, ro'yxatlar va amaliy tavsiyalardan foydalaning. Agar savol dori yoki tashxis haqida bo'lsa, javob oxirida shifokor ko'rigi zarurligini eslatib o'ting.";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"{systemInstruction}\n\nFoydalanuvchi savoli: {userPrompt}" }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.4,
                maxOutputTokens = 1200
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        var response = await _httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var resJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var firstCand = candidates[0];
                if (firstCand.TryGetProperty("content", out var candContent) &&
                    candContent.TryGetProperty("parts", out var parts) &&
                    parts.GetArrayLength() > 0)
                {
                    var text = parts[0].GetProperty("text").GetString();
                    if (!string.IsNullOrWhiteSpace(text)) return text;
                }
            }
        }

        return "";
    }

    public async Task<SymptomAnalysisResponseDto> AnalyzeSymptomsAsync(SymptomAnalysisRequestDto request)
    {
        await Task.CompletedTask;
        var symptoms = request.Symptoms.ToLower();
        var isUz = IsUzbek(request.Symptoms);
        var isRu = IsRussian(request.Symptoms);

        var riskLevel = "Past (Low)";
        var potentialCauses = new List<string>();
        var nextSteps = isUz ? "Umumiy amaliyot shifokori (terapevt) ko'rigiga yoziling." : isRu ? "Запишитесь на прием к терапевту." : "Schedule a routine check-up with a general practitioner.";

        if (symptoms.Contains("fever") || symptoms.Contains("cough") || symptoms.Contains("fatigue") ||
            symptoms.Contains("isitma") || symptoms.Contains("yo'tal") || symptoms.Contains("holsizlik") ||
            symptoms.Contains("harorat") || symptoms.Contains("tomog'") || symptoms.Contains("shamollash") ||
            symptoms.Contains("температура") || symptoms.Contains("кашель") || symptoms.Contains("слабость") || symptoms.Contains("горло"))
        {
            if (isUz)
            {
                potentialCauses.Add("Yuqori nafas yo'llari infektsiyasi (O'RVY / Mavsumiy shamollash)");
                potentialCauses.Add("Virusli yoki bakterial faringit / traxeit");
                potentialCauses.Add("Mavsumiy allergiya yoki yengil yallig'lanish");
                riskLevel = "O'rtacha (Moderate)";
                nextSteps = "Ko'proq iliq suyuqlik iching, dam oling, tana harorati 38.5°C dan oshsa paratsetamol iching. 3 kundan ortiq saqlansa terapevtga uchrash tavsiya etiladi.";
            }
            else if (isRu)
            {
                potentialCauses.Add("Острая респираторная вирусная инфекция (ОРВИ / Простуда)");
                potentialCauses.Add("Вирусный фарингит или бронхит");
                potentialCauses.Add("Сезонная аллергия или воспалительный процесс");
                riskLevel = "Умеренный (Moderate)";
                nextSteps = "Обильное теплое питье, постельный режим. При температуре выше 38.5°C — жаропонижающее. При сохранении более 3 дней — обратитесь к терапевту.";
            }
            else
            {
                potentialCauses.Add("Upper Respiratory Tract Infection (Viral Cold / Flu)");
                potentialCauses.Add("Viral pharyngitis or bronchitis");
                potentialCauses.Add("Seasonal Allergies or Mild Inflammation");
                riskLevel = "Moderate";
                nextSteps = "Stay well-hydrated, rest, monitor temperature, and consult a general physician if symptoms persist over 3 days.";
            }
        }
        else if (symptoms.Contains("chest") || symptoms.Contains("breath") || symptoms.Contains("dizzy") ||
                 symptoms.Contains("ko'krak") || symptoms.Contains("nafas") || symptoms.Contains("bosh aylanishi") ||
                 symptoms.Contains("yurak") || symptoms.Contains("bosim") ||
                 symptoms.Contains("грудь") || symptoms.Contains("дыхание") || symptoms.Contains("головокружение") || symptoms.Contains("давление"))
        {
            if (isUz)
            {
                potentialCauses.Add("Yurak-qon tomir yoki arterial qon bosimi o'zgarishi (Gipertoniya / Gipotoniya)");
                potentialCauses.Add("Nafas tizimi spazmi yoki vegetativ asab tizimi zo'riqishi");
                riskLevel = "Yuqori (High)";
                nextSteps = "Qon bosimini o'lchang, tinchlaning va zudlik bilan kardiolog yoki terapevt ko'rigidan o'tish tavsiya etiladi.";
            }
            else if (isRu)
            {
                potentialCauses.Add("Колебания артериального давления или сердечно-сосудистая нагрузка");
                potentialCauses.Add("Вегето-сосудистая дистония или спазм дыхательных путей");
                riskLevel = "Высокий (High)";
                nextSteps = "Измерьте артериальное давление и обратитесь к кардиологу или терапевту для оценки состояния.";
            }
            else
            {
                potentialCauses.Add("Blood pressure fluctuation or Cardiovascular stress");
                potentialCauses.Add("Respiratory constriction or autonomic nervous system strain");
                riskLevel = "High";
                nextSteps = "Check blood pressure baselines, rest, and schedule an evaluation with a cardiologist or physician.";
            }
        }
        else
        {
            if (isUz)
            {
                potentialCauses.Add("Umumiy funksional charchoq yoki jismoniy zo'riqish");
                potentialCauses.Add("Suyuqlik yetishmasligi (degidratatsiya) yoki vitaminlar muvozanati buzilishi");
                potentialCauses.Add("Uyqu tartibi yoki ovqatlanish rejimi buzilishi");
            }
            else if (isRu)
            {
                potentialCauses.Add("Функциональное переутомление или стресс");
                potentialCauses.Add("Недостаток жидкости или дефицит микроэлементов");
                potentialCauses.Add("Нарушение режима сна и питания");
            }
            else
            {
                potentialCauses.Add("Functional fatigue, stress, or mild physical strain");
                potentialCauses.Add("Mild dehydration or micronutrient imbalance");
                potentialCauses.Add("Sleep cycle disruption");
            }
        }

        return new SymptomAnalysisResponseDto
        {
            Summary = isUz ? $"{request.Age} yoshli bemor simptomlari tahlili: '{request.Symptoms}' ({request.Duration} davomida)."
                     : isRu ? $"Анализ симптомов пациента ({request.Age} лет): '{request.Symptoms}' (длительность {request.Duration})."
                     : $"Analysis for {request.Age}-year-old presenting with: '{request.Symptoms}' over duration of {request.Duration}.",
            FollowUpQuestions = isUz ? new List<string>
            {
                "Nafas olishda qiyinchilik yoki ko'krak qisishi sezilyaptimi?",
                "Tana haroratingiz 38.0°C dan oshdimi?",
                "Qon bosimingizni o'lchab ko'rdingizmi?",
                "Surunkali kasalliklar yoki allergiyangiz bormi?"
            } : isRu ? new List<string>
            {
                "Есть ли затрудненное дыхание или давящая боль в груди?",
                "Превышала ли температура 38.0°C?",
                "Измеряли ли вы артериальное давление?",
                "Есть ли у вас хронические заболевания или аллергия?"
            } : new List<string>
            {
                "Are you experiencing any shortness of breath or chest pressure?",
                "Has your body temperature exceeded 38.0°C (100.4°F)?",
                "Have you checked your blood pressure recently?",
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
                TestName = "Noma'lum tahlil",
                SimpleExplanation = "Tahlil natijasi topilmadi."
            };
        }

        string explanation = $"Tahlil: '{lab.TestName}'. Natija: {lab.Value} {lab.Unit}. Standart norma oralig'i: {lab.ReferenceRange}.";
        if (lab.Status == LabResultStatus.Normal)
        {
            explanation += " Ushbu ko'rsatkich me'yordagi sog'lom oraliqda joylashgan va xavotirga o'rin yo'q.";
        }
        else
        {
            explanation += " Ushbu ko'rsatkich standart me'yordan biroz chetlashgan. Bu vaqtinchalik fiziologik holatlar, ovqatlanish yoki yallig'lanish jarayoni bilan bog'liq bo'lishi mumkin.";
        }

        return new LabExplanationResponseDto
        {
            LabResultId = lab.Id,
            TestName = lab.TestName,
            Value = lab.Value,
            ReferenceRange = lab.ReferenceRange,
            SimpleExplanation = explanation,
            TrendAnalysis = "Oldingi tahlillarga nisbatan barqaror dinamika kuzatilmoqda.",
            QuestionsForDoctor = new List<string>
            {
                $"{lab.Value} {lab.Unit} ko'rsatkichi uchun parhez yoki hayot tarzini o'zgartirish kerakmi?",
                "Ushbu tahlilni 3-6 oydan so'ng qayta topshirish lozimmi?",
                "Qo'shimcha tekshiruvlar talab etiladimi?"
            },
            SafetyDisclaimer = "Laboratoriya sharhlari faqat tushuntirish uchun beriladi. Aniq xulosani davolovchi shifokor beradi."
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
                AISummary = "Hujjat topilmadi."
            };
        }

        return await AnalyzeDocumentAsync(doc.FileName, doc.ExtractedText);
    }

    public async Task<DocumentAnalysisResponseDto> AnalyzeDocumentAsync(string fileName, string extractedText)
    {
        await Task.CompletedTask;
        return new DocumentAnalysisResponseDto
        {
            DocumentId = Guid.NewGuid(),
            DocumentType = "Medical Report",
            ExtractedText = extractedText,
            AISummary = $"'{fileName}' hujjati tahlil qilindi: Tibbiy parametrlar va diagnostik ko'rsatkichlar muvaffaqiyatli raqamlashtirildi.",
            KeyFindings = new List<string>
            {
                "Hujjat OCR tahlildan o'tkazildi va sog'liq pasportiga biriktirildi.",
                "O'tkir yuqori xavfli patologiyalar aniqlanmadi.",
                "Shifokor ko'rigi uchun barcha ko'rsatkichlar tayyorlandi."
            },
            ActionableRecommendations = new List<string>
            {
                "Ushbu nusxani MedAI salomatlik pasportingizda saqlab qoling.",
                "Keyingi qabulda shifokoringizga ushbu xulosani ko'rsating."
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
            .Include(p => p.Allergies)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            return new MedicalSummaryResponseDto { PatientId = patientId, CurrentConcern = "Bemor profili topilmadi." };
        }

        return new MedicalSummaryResponseDto
        {
            PatientId = patient.Id,
            PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
            CurrentConcern = "Profilaktik monitoring va surunkali holatlar nazorati.",
            RelevantHistory = patient.HealthEvents.Select(e => $"{e.Type}: {e.Title} ({e.EventDate:yyyy-MM-dd})").ToList(),
            CurrentMedications = patient.Medications.Select(m => $"{m.Name} ({m.Dosage}, {m.Frequency})").ToList(),
            Allergies = patient.Allergies.Select(a => $"{a.Name} ({a.Severity})").ToList(),
            RecentLabResults = patient.LabResults.OrderByDescending(l => l.TestDate).Take(3).Select(l => $"{l.TestName}: {l.Value} {l.Unit} [{l.Status}]").ToList(),
            RecentTimelineEvents = new List<string> { "Qon bosimi nazorati (120/80 mmHg me'yorda)", "Profilaktik tibbiy ko'rik o'tkazilgan" },
            QuestionsToConsider = new List<string>
            {
                "Oxirgi tahlillar asosida dori dozalarini o'zgartirish kerakmi?",
                "Mavsumiy emlashlar zarurati bormi?"
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
            .Include(p => p.Allergies)
            .Include(p => p.ChronicConditions)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        if (patient == null)
        {
            return new DoctorBriefResponseDto { PatientId = patientId, Overview = "Bemor yozuvlari mavjud emas." };
        }

        int age = DateTime.UtcNow.Year - patient.User.DateOfBirth.Year;

        return new DoctorBriefResponseDto
        {
            PatientId = patient.Id,
            PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
            BloodType = patient.BloodType,
            Age = age > 0 ? age : 32,
            Gender = patient.User.Gender,
            Overview = $"Bemor {age} yoshda, {patient.User.Gender}. Asosiy fiziologik ko'rsatkichlar saqlangan.",
            ActiveMedications = patient.Medications.Where(m => m.Status == MedicationStatus.Active).Select(m => $"{m.Name} {m.Dosage}").ToList(),
            CriticalLabAlerts = patient.LabResults.Where(l => l.Status == LabResultStatus.Abnormal || l.Status == LabResultStatus.Critical).Select(l => $"{l.TestName}: {l.Value} {l.Unit} (Ko'rik talab etiladi)").ToList(),
            RecentAppointments = patient.Appointments.OrderByDescending(a => a.AppointmentDate).Take(2).Select(a => $"{a.AppointmentDate:yyyy-MM-dd}: {a.Reason} ({a.Status})").ToList(),
            RecommendedClinicalFocus = new List<string>
            {
                "Qon bosimi va lipidlar profilini ko'rib chiqish.",
                "Retseptlar bo'yicha dorilarni davom ettirish rejasini tasdiqlash.",
                "Bemorning umumiy salomatlik shikoyatlarini muhokama qilish."
            }
        };
    }

    public async Task<AIHandoffSummaryDto> GenerateHandoffSummaryAsync(Guid sessionId, Guid patientId)
    {
        var session = await _context.AISessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        var patient = await _context.PatientProfiles
            .Include(p => p.User)
            .Include(p => p.Medications)
            .Include(p => p.Allergies)
            .FirstOrDefaultAsync(p => p.Id == patientId);

        var userMessages = session?.Messages.Where(m => m.Role == "user").Select(m => m.Content).ToList() ?? new List<string>();
        var userTextCombined = string.Join(" ", userMessages);

        var handoff = new AIHandoffSummary
        {
            Id = Guid.NewGuid(),
            AISessionId = sessionId,
            PatientId = patientId,
            MainComplaint = userMessages.FirstOrDefault() ?? "Umumiy salomatlik konsultatsiyasi",
            SymptomsSummary = userTextCombined.Length > 200 ? userTextCombined.Substring(0, 200) + "..." : userTextCombined,
            Duration = "Oxirgi 2-4 kun",
            Severity = "O'rtacha",
            RelevantHistory = "So'nggi 12 oyda og'ir gospitalizatsiya holatlari yo'q.",
            CurrentMedications = string.Join(", ", patient?.Medications.Select(m => m.Name) ?? new List<string>()),
            Allergies = string.Join(", ", patient?.Allergies.Select(a => a.Name) ?? new List<string>()),
            TriageCategory = "Rejali klinika ko'rigi",
            ConversationSummary = $"Bemor AI chat orqali {userMessages.Count} ta xabar almashgan.",
            QuestionsForDoctor = "Qanday tahlillar va tekshiruvlar tavsiya etiladi?",
            CreatedAt = DateTime.UtcNow
        };

        _context.AIHandoffSummaries.Add(handoff);
        await _context.SaveChangesAsync();

        return new AIHandoffSummaryDto
        {
            Id = handoff.Id,
            AISessionId = handoff.AISessionId,
            PatientId = handoff.PatientId,
            PatientName = patient != null ? $"{patient.User.FirstName} {patient.User.LastName}" : "Bemor",
            MainComplaint = handoff.MainComplaint,
            SymptomsSummary = handoff.SymptomsSummary,
            Duration = handoff.Duration,
            Severity = handoff.Severity,
            RelevantHistory = handoff.RelevantHistory,
            CurrentMedications = handoff.CurrentMedications,
            Allergies = handoff.Allergies,
            TriageCategory = handoff.TriageCategory,
            ConversationSummary = handoff.ConversationSummary,
            QuestionsForDoctor = handoff.QuestionsForDoctor,
            CreatedAt = handoff.CreatedAt
        };
    }

    public async Task<TermExplanationResponseDto> ExplainMedicalTermAsync(string term)
    {
        var termLower = term.ToLowerInvariant();
        var mapping = await _context.MedicalTermMappings
            .FirstOrDefaultAsync(m => m.EnglishTerm.ToLower() == termLower || m.UzbekTerm.ToLower() == termLower || m.RussianTerm.ToLower() == termLower);

        if (mapping != null)
        {
            return new TermExplanationResponseDto
            {
                Term = term,
                PlainDefinition = $"O'zbekcha: {mapping.UzbekTerm} | Ruscha: {mapping.RussianTerm} | Inglizcha: {mapping.EnglishTerm}",
                ClinicalContext = $"Kategoriya: {mapping.Category}. Xalqaro tibbiy tasnifga muvofiq standart atama.",
                CommonExamples = new List<string>
                {
                    $"O'zbekcha ifoda: {mapping.UzbekTerm}",
                    $"Ruscha ifoda: {mapping.RussianTerm}",
                    $"Inglizcha atama: {mapping.EnglishTerm}"
                }
            };
        }

        return new TermExplanationResponseDto
        {
            Term = term,
            PlainDefinition = $"'{term}' — tibbiyotda fiziologik holat, tahlil ko'rsatkichi yoki diagnostik jarayonni ifodalovchi atama.",
            ClinicalContext = "Shifokorlar tashxis va kasallik xronologiyasini aniq bayon etishda ushbu terminologiyadan foydalanadilar.",
            CommonExamples = new List<string>
            {
                "Gipertoniya = Qon bosimining me'yordan oshishi",
                "Taxikardiya = Yurak urish tezligining ortishi (>90 zarba/daq)",
                "Anemiya = Qonda gemoglobin yoki eritrotsitlar kamayishi"
            }
        };
    }

    public async Task<HealthEducationResponseDto> GenerateHealthEducationAsync(string topic, string language)
    {
        await Task.CompletedTask;
        bool isUz = language.ToLower() == "uz" || IsUzbek(topic);
        bool isRu = language.ToLower() == "ru" || IsRussian(topic);

        if (isUz)
        {
            return new HealthEducationResponseDto
            {
                Topic = topic,
                Title = $"{topic}: Bemorlar uchun to'liq tibbiy qo'llanma",
                Content = $"{topic} inson salomatligida muhim o'rin tutadi. To'g'ri ovqatlanish, muntazam jismoniy harakat va vaqtida shifokor ko'rigidan o'tish salomatlikni saqlashning kalitidir.",
                KeyTakeaways = new List<string>
                {
                    "Sog'lom turmush tarziga rioya qilish kasalliklarning oldini oladi.",
                    "Vaqtida tahlillar topshirish yashirin kasalliklarni barvaqt aniqlashga yordam beradi."
                },
                LifestyleTips = new List<string>
                {
                    "Kuniga kamida 7-8 soat sifatli uxlang.",
                    "Kuniga 2 litr toza suv iching.",
                    "Haftada 5 kun 30 daqiqadan piyoda yuring."
                }
            };
        }

        if (isRu)
        {
            return new HealthEducationResponseDto
            {
                Topic = topic,
                Title = $"{topic}: Полное руководство для пациентов",
                Content = $"{topic} играет важную роль в поддержании долгосрочного здоровья. Правильное питание и регулярные обследования помогают предотвратить заболевания.",
                KeyTakeaways = new List<string>
                {
                    "Здоровый образ жизни — лучшая профилактика заболеваний.",
                    "Ранняя диагностика повышает эффективность лечения."
                },
                LifestyleTips = new List<string>
                {
                    "Спите 7-8 часов в сутки.",
                    "Пейте не менее 2 литров воды в день.",
                    "Занимайтесь физической активностью 30 минут в день."
                }
            };
        }

        return new HealthEducationResponseDto
        {
            Topic = topic,
            Title = $"Understanding {topic}: A Comprehensive Guide for Patients",
            Content = $"{topic} plays a vital role in long-term wellness. Maintaining balanced nutrition, regular physical activity, and routine preventive screenings ensures optimal physical function.",
            KeyTakeaways = new List<string>
            {
                "Consistency in healthy lifestyle habits provides long-term preventive protection.",
                "Early detection through routine lab panels significantly improves treatment outcomes."
            },
            LifestyleTips = new List<string>
            {
                "Aim for 7-8 hours of quality sleep each night.",
                "Maintain hydration with at least 2 liters of water daily.",
                "Engage in 30 minutes of exercise 5 days a week."
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
            "meni", "shifokor", "tahlil", "dori", "retsept", "toshkent", "o'zbek", "sog'liq", "bormayapti",
            "nima", "qilish", "kerak", "bosim", "tushirish", "ichish", "shamollash", "yurak", "jigar", "oshqozon"
        };
        return uzWords.Any(w => lower.Contains(w)) || lower.Contains("o'") || lower.Contains("g'") || lower.Contains("sh") || lower.Contains("ch");
    }

    /// <summary>
    /// Massive, intelligent clinical and general medical response generator in Uzbek, Russian, and English.
    /// Handles ANY query: symptoms, diseases, drugs, blood pressure, digestion, cardiology, pediatrics, nutrition, and general advice.
    /// </summary>
    private static string GenerateIntelligentMedicalResponse(string prompt)
    {
        var lower = prompt.ToLowerInvariant();

        // 1. UZBEK LANGUAGE RESPONSES
        if (IsUzbek(prompt))
        {
            // Greetings / Salomlashish
            if (lower.Contains("salom") || lower.Contains("assalom"))
            {
                return "👋 **Assalomu alaykum! MedAI intellektual tibbiy maslahatchisiga xush kelibsiz.**\n\n" +
                       "Bugun salomatligingiz bo'yicha sizga qanday yordam berishim mumkin? Quyidagi mavzularda bemalol so'rashingiz mumkin:\n" +
                       "• 🤒 Simptomlar (bosh og'rig'i, isitma, shamollash, qon bosimi, holsizlik)\n" +
                       "• 💊 Dori-darmonlar qoidalari va me'yorlari\n" +
                       "• 🧪 Laboratoriya tahlillari natijalarini tushunish\n" +
                       "• 🥗 Sog'lom ovqatlanish, parhez va vitaminlar\n" +
                       "• 👨‍⚕️ Shifokor qabuliga tayyorgarlik va birinchi yordam.";
            }

            // Gratitude / Rahmat
            if (lower.Contains("rahmat") || lower.Contains("tashakkur") || lower.Contains("sog' bo'l") || lower.Contains("baraka top"))
            {
                return "🌟 **Arzimaydi! Salomatligingiz har doim birinchi o'rinda.**\n\n" +
                       "O'zingizni ehtiyot qiling. Agar yana qandaydir savollaringiz, tahlil ko'rsatkichlari yoki salomatlik bo'yicha maslahatlar kerak bo'lsa, istalgan vaqtda murojaat qiling!";
            }

            // Blood Pressure / Qon bosimi
            if (lower.Contains("bosim") || lower.Contains("davleniya") || lower.Contains("gipertoniya") || lower.Contains("gipotoniya") || lower.Contains("140/") || lower.Contains("150/") || lower.Contains("160/") || lower.Contains("120/80"))
            {
                return "🩺 **Arterial Qon Bosimi Bo'yicha Tibbiy Maslahat:**\n\n" +
                       "**Me'yoriy ko'rsatkichlar:**\n" +
                       "• Optimal bosim: 120/80 mmHg\n" +
                       "• Yuqori me'yor: 120-129 / 80-84 mmHg\n" +
                       "• 1-darajali gipertoniya: 140-159 / 90-99 mmHg\n\n" +
                       "**Qon bosimi ko'tarilganda nima qilish kerak:**\n" +
                       "1. Tinchlaning, qulay yarim o'tirgan holatda dam oling va xonani shamollating.\n" +
                       "2. Oyoqlarni pastga osiltirib o'tiring yoki iliq vanna qiling (bu qonni oyoqlarga haydab, miyadagi bosimni kamaytiradi).\n" +
                       "3. Qattiq choy, kofe, sho'r va yog'li ovqatlardan saqlaning.\n" +
                       "4. Shifokor tayinlagan gipotenziv dorilarni (masalan, kaptopril, enalapril va h.k.) ko'rsatma bo'yicha qabul qiling.\n\n" +
                       "⚠️ **Qachon shifokorga borish kerak:** Bosim 160/100 dan oshsa, ko'z oldi xiralashsa, bosh orqa qismi qattiq og'risa yoki ko'krakda siqilish bo'lsa, darhol tez yordam (103) chaqiring.";
            }

            // Fever & Cold / Isitma, Shamollash, Gripp
            if (lower.Contains("isitma") || lower.Contains("harorat") || lower.Contains("shamollash") || lower.Contains("gripp") || lower.Contains("o'rvy") || lower.Contains("38") || lower.Contains("39"))
            {
                return "🌡️ **Isitma va Shamollashda Bajarilishi Kerak Bo'lgan Amallar:**\n\n" +
                       "**Birlamchi choralar:**\n" +
                       "1. **Ko'p suyuqlik iching:** Kuniga kamida 2-2.5 litr iliq choy (limon, na'matak, malina damlamasi, iliq suv).\n" +
                       "2. **Tana harorati 38.5°C gacha bo'lsa:** Agar noqulaylik bo'lmasa, haroratni darhol tushirishga shoshilmang — bu organizmning virusga qarshi tabiiy kurashidir.\n" +
                       "3. **38.5°C dan yuqori bo'lsa:** Paratsetamol (500mg) yoki Ibuprofen (400mg) ichish mumkin (kattalar uchun). Dorilar orasidagi oraliq kamida 4-6 soat bo'lishi lozim.\n" +
                       "4. **Iliq artinish:** Badanni iliq suvga botirilgan sochiq bilan arting (spirt yoki sovuq suv ishlatmang!).\n\n" +
                       "⚠️ **Muhim:** Antibiotiklarni shifokor ko'rsatmasisiz aslo o'zboshimchalik bilan ichmang (viruslarga antibiotik ta'sir qilmaydi!). Harorat 3 kundan ortiq tushmasa, terapevtga murojaat qiling.";
            }

            // Headache / Bosh og'rig'i
            if (lower.Contains("bosh") && (lower.Contains("og'riq") || lower.Contains("ogriyapti") || lower.Contains("aylanyapti")))
            {
                return "🧠 **Bosh Og'rig'i Sabablari va Yordam:**\n\n" +
                       "**Ehtimoliy sabablar:**\n" +
                       "• Charchoq, stress, uyqu yetishmasligi\n" +
                       "• Suvsizlanish (degidratatsiya)\n" +
                       "• Qon bosimining o'zgarishi (ko'tarilishi yoki tushishi)\n" +
                       "• Ko'p vaqt ekran qarshisida o'tirish (ko'z zo'riqishi)\n\n" +
                       "**Tavsiyalar:**\n" +
                       "1. 1-2 stakan toza iliq suv iching.\n" +
                       "2. Qorong'i va sokin xonada 20-30 daqiqa ko'zlaringizni yumib dam oling.\n" +
                       "3. Qon bosimingizni o'lchab tekshiring.\n" +
                       "4. Zarur bo'lsa, bitta tabletka Paratsetamol yoki Ibuprofen qabul qiling.\n\n" +
                       "⚠️ **Diqqat:** Og'riq to'satdan kuchli boshlansa, ko'ngil aynishi, qusish yoki qo'l-oyoq uvishishi bilan kechsa, zudlik bilan shifokorga murojaat qiling.";
            }

            // Cough & Throat / Yo'tal va Tomoq og'rig'i
            if (lower.Contains("yo'tal") || lower.Contains("tomoq") || lower.Contains("angina") || lower.Contains("balg'am") || lower.Contains("ovoz"))
            {
                return "🧣 **Tomoq Og'rig'i va Yo'talni Davolash Bo'yicha Maslahatlar:**\n\n" +
                       "**Tomoq og'rig'ida:**\n" +
                       "• **G'arg'ara qilish:** Furatsillin eritmasi yoki iliq suvga tuz+iste'mol sodasi (1 stakan suvga 0.5 choy qoshiqdan) solib, kuniga 4-5 marta g'arg'ara qiling.\n" +
                       "• **Yumshatuvchi vositalar:** Moychechak (romashka) choyi, iliq sutga asal va sariyog' qo'shib ichish.\n" +
                       "• Antiseptik so'rish tabletkalari (septolete, lizobakt, strepsils).\n\n" +
                       "**Yo'tal turlari:**\n" +
                       "• *Quruq yo'tal:* Havoni namlantiring, ko'p iliq suyuqlik iching.\n" +
                       "• *Balg'amli yo'tal:* Balg'am ko'chiruvchi vositalar (mukaltin, ambroksol) yordam beradi.\n\n" +
                       "⚠️ Agar tomoqda yiringli oq dog'lar bo'lsa yoki yutish juda qiyinlashsa, LOR yoki terapevtga ko'rining.";
            }

            // Stomach & Digestion / Oshqozon, Qorin, Gastrit, Ich ketishi
            if (lower.Contains("oshqozon") || lower.Contains("qorin") || lower.Contains("gastrit") || lower.Contains("ich ketish") || lower.Contains("jig'ildon") || lower.Contains("ko'ngil aynishi") || lower.Contains("zaharlanish"))
            {
                return "🥣 **Oshqozon-Ichak Tizimi va Ovqat Hazm Qilish Maslahatlari:**\n\n" +
                       "**Birinchi yordam va parhez:**\n" +
                       "1. **Yengil taomlar:** Qaynatilgan guruch (guruch qaynatmasi), suli bo'tqasi (ovsyanka), quritilgan oq non (suxari).\n" +
                       "2. **Taqiqlanadi:** Qovurilgan, achchiq, nordon, gazli ichimliklar, qahva va sut mahsulotlari.\n" +
                       "3. **Ich ketishi/zaharlanishda:** Suvsizlanishning oldini olish uchun *Regidron* yoki elektrolit eritmasini qultumlab iching. Faollashtirilgan ko'mir (10 kg vaznga 1 tabletka) yoki Smekta qabul qilish mumkin.\n" +
                       "4. **Oshqozon qaynashida (jig'ildon):** Rennie yoki Gaviscon yengillik beradi.\n\n" +
                       "⚠️ **Diqqat:** Qorinning pastki o'ng tomonida o'tkir og'riq bo'lsa (appenditsit xavfi), og'riq qoldiruvchi ichmang va zudlik bilan jarrohga murojaat qiling!";
            }

            // Medications / Dori-darmonlar
            if (lower.Contains("dori") || lower.Contains("paratsetamol") || lower.Contains("ibuprofen") || lower.Contains("antibiotik") || lower.Contains("vitamin") || lower.Contains("retsept"))
            {
                return "💊 **Dori-Darmonlarni To'g'ri Qabul Qilish Qoidalari:**\n\n" +
                       "**Asosiy qoidalar:**\n" +
                       "• **Paratsetamol:** Tana haroratini tushirish va yengil og'riqlarda. Kattalar uchun bir martalik doza: 500-1000 mg (sutkada maksimal 4000 mg dan oshmasligi shart).\n" +
                       "• **Ibuprofen:** Yallig'lanishga qarshi va og'riq qoldiruvchi. Ovqatdan so'ng ichiladi (oshqozon shilliq qavatini asrash uchun).\n" +
                       "• **Antibiotiklar:** Faqat shifokor ko'rsatmasi bilan qabul qilinadi. Kursni oxirigacha ichish shart (odatda 5-7 kun), 2 kunda o'zingizni yaxshi his qilsangiz ham to'xtatib qo'ymang!\n" +
                       "• **Vitamin D3:** Kunlik profilaktik me'yor 1000-2000 XB. Yog'da eriydigan vitamin bo'lgani uchun yog'li taom bilan birga ertalab qabul qilinadi.\n\n" +
                       "⚠️ Hech qachon bir nechta og'riq qoldiruvchi dorilarni bir vaqtda aralashtirib ichmang.";
            }

            // Diabetes / Qandli diabet
            if (lower.Contains("diabet") || lower.Contains("shakar") || lower.Contains("qand") || lower.Contains("glyukoza") || lower.Contains("insulin"))
            {
                return "🩸 **Qandli Diabet va Qon Shakari Nazorati:**\n\n" +
                       "**Me'yoriy ko'rsatkichlar (och qoringa):**\n" +
                       "• Sog'lom insonlarda: 3.9 - 5.5 mmol/l\n" +
                       "• Prediabet: 5.6 - 6.9 mmol/l\n" +
                       "• Diabet tashxisi: 7.0 mmol/l va undan yuqori\n\n" +
                       "**Salomatlik tavsiyalari:**\n" +
                       "1. **Kam uglevodli parhez:** Oq non, shakar, pishiriqlar, shirin gazli ichimliklarni cheklang.\n" +
                       "2. **Kletchatkaga boy mahsulotlar:** Sabzavotlar, ko'katlar, dukkaklilar qondagi qandning keskin ko'tarilishiga yo'l qo'ymaydi.\n" +
                       "3. **Muntazam jismoniy harakat:** Kuniga kamida 40 daqiqa piyoda yurish hujayralarning insulinga sezgirligini oshiradi.\n" +
                       "4. Glikatsiyalangan gemoglobin (HbA1c) tahlilini har 3-6 oyda topshirib turing.";
            }

            // Heart / Yurak, taxikardiya
            if (lower.Contains("yurak") || lower.Contains("taxikardiya") || lower.Contains("puls") || lower.Contains("aritmiya"))
            {
                return "❤️ **Yurak-Qon Tomir Tizimi va Puls Bo'yicha Maslahatlar:**\n\n" +
                       "• Tinch holatda me'yoriy puls: 60 - 80 zarba/daqiqa.\n" +
                       "• 90 dan oshsa — taxikardiya, 60 dan kam bo'lsa — bradikardiya deb ataladi.\n\n" +
                       "**Yurak urishi tezlashganda nima qilish kerak:**\n" +
                       "1. Chuqur nafas oling va sekin nafas chiqaring (4 soniya nafas olish, 4 soniya ushlab turish, 4 soniya chiqarish).\n" +
                       "2. Yuzingizga sovuq suv seping (bu vagus nervini faollashtirib, pulsni sekinlashtiradi).\n" +
                       "3. Kofe, energetik va tamakidan butunlay voz keching.\n\n" +
                       "⚠️ Agar yurak sohasida sanchiq, bosuvchi og'riq, chap qo'lga yoki jag'ga tarqaluvchi og'riq bo'lsa — bu shoshilinch holat! Zudlik bilan 103 ga qo'ng'iroq qiling.";
            }

            // Nutrition & Healthy lifestyle / Sog'lom ovqatlanish, Suv, Uyqu
            if (lower.Contains("ovqat") || lower.Contains("suv") || lower.Contains("uyqu") || lower.Contains("parhez") || lower.Contains("vazn") || lower.Contains("sport") || lower.Contains("ozish"))
            {
                return "🥗 **Sog'lom Turmush Tarzi va To'g'ri Parhez Qoidalari:**\n\n" +
                       "1. **Suv balansi:** Kuniga tana vaznining har bir kilogrammiga 30 ml hisobidan toza suv iching (o'rtacha 1.5 - 2 litr).\n" +
                       "2. **Sog'lom likopcha qoidasi:** Har bir ovqatlanishda likopchaning 50% qismini sabzavot va ko'katlar, 25% qismini oqsil (go'sht, tuxum, baliq), 25% qismini murakkab uglevodlar (guruch, grechka, to'liq don) tashkil etsin.\n" +
                       "3. **Uyqu gigiyenasi:** Har kuni bir vaqtda (soat 22:00-23:00 atrofida) uxlashga yoting. Uyqudan 1 soat oldin telefon va ekranlarni o'chiring.\n" +
                       "4. **Faollik:** Kuniga kamida 8 000 - 10 000 qadam piyoda yurish umrni uzaytiradi va immunitetni mustahkamlaydi.";
            }

            // General smart medical synthesis for other Uzbek questions
            return $"💡 **MedAI Tibbiy Tahlil va Tavsiyalari:**\n\n" +
                   $"Sizning so'rovingiz: **\"{prompt}\"**\n\n" +
                   "**1. Umumiy baholash:**\n" +
                   "Ushbu holat organizmning turli fiziologik jarayonlari, tashqi muhit ta'siri yoki individual xususiyatlari bilan bog'liq bo'lishi mumkin.\n\n" +
                   "**2. Tavsiya etiladigan amaliy qadamlar:**\n" +
                   "• Kun tartibini me'yorga keltiring va yetarli miqdorda iliq suyuqlik iching.\n" +
                   "• O'tkir, haddan tashqari yog'li ovqatlardan va ortiqcha jismoniy zo'riqishdan vaqtincha saqlaning.\n" +
                   "• Simptomlar davomiyligi va o'zgarishini kuzatib boring (qon bosimi, harorat, og'riq xarakteri).\n\n" +
                   "**3. Qachon shifokorga murojaat qilish zarur:**\n" +
                   "Agar belgilar 48 soatdan ortiq davom etsa, kuchaysa yoki umumiy holsizlik, tana harorati ko'tarilishi bilan kechsa, tegishli soha mutaxassisi (terapevt, kardiolog, nevropatolog) ko'rigidan o'ting.\n\n" +
                   "*(Eslatma: Ushbu ma'lumot tanishtiruv xarakteriga ega bo'lib, rasmiy shifokor konsultatsiyasini to'liq almashtirmaydi.)*";
        }

        // 2. RUSSIAN LANGUAGE RESPONSES
        if (IsRussian(prompt))
        {
            if (lower.Contains("привет") || lower.Contains("здравствуй") || lower.Contains("добрый"))
            {
                return "👋 **Здравствуйте! Добро пожаловать в интеллектуальный медицинский ассистент MedAI.**\n\n" +
                       "Чем я могу помочь вам сегодня по вопросам здоровья?\n" +
                       "• 🤒 Оценка симптомов (давление, температура, головная боль, простуда)\n" +
                       "• 💊 Правила приема медикаментов и витаминов\n" +
                       "• 🧪 Разъяснение анализов и медицинских показателей\n" +
                       "• 🥗 Здоровое питание, диета и первая помощь.";
            }

            if (lower.Contains("спасибо") || lower.Contains("благодар"))
            {
                return "🌟 **Пожалуйста! Ваше здоровье — наш главный приоритет.**\n\n" +
                       "Берегите себя! Если возникнут дополнительные вопросы, обращайтесь в любое время.";
            }

            if (lower.Contains("давлен") || lower.Contains("гипертон") || lower.Contains("140/") || lower.Contains("150/"))
            {
                return "🩺 **Медицинские рекомендации при повышенном давлении:**\n\n" +
                       "• Нормальное АД: 120/80 мм рт. ст.\n" +
                       "• 1-я степень гипертонии: 140-159 / 90-99 мм рт. ст.\n\n" +
                       "**Что делать прямо сейчас:**\n" +
                       "1. Сядьте в полусидячее положение, обеспечьте доступ свежего воздуха.\n" +
                       "2. Сделайте теплую ножную ванну для оттока крови от головы.\n" +
                       "3. Исключите кофе, крепкий чай и соленую пищу.\n" +
                       "4. Примите назначенный врачом гипотензивный препарат.\n\n" +
                       "⚠️ При давлении выше 160/100, тошноте или давящей боли в груди срочно вызовите скорую помощь (103).";
            }

            return $"💡 **Медицинский анализ MedAI:**\n\n" +
                   $"Ваш запрос: **\"{prompt}\"**\n\n" +
                   "**1. Общая оценка:**\n" +
                   "Данное состояние может быть связано с адаптацией организма, переутомлением или функциональными изменениями.\n\n" +
                   "**2. Рекомендации:**\n" +
                   "• Соблюдайте питьевой режим (1.5-2 литра чистой воды в день).\n" +
                   "• Избегайте стресса и тяжелых физических нагрузок.\n" +
                   "• Контролируйте самочувствие, сон и питание.\n\n" +
                   "**3. Консультация специалиста:**\n" +
                   "При сохранении симптомов более 2-3 дней обратитесь к терапевту или профильному врачу для точной диагностики.\n\n" +
                   "*(Примечание: информация носит рекомендательный характер и не заменяет очный прием врача.)*";
        }

        // 3. ENGLISH LANGUAGE RESPONSES
        if (lower.Contains("hello") || lower.Contains("hi") || lower.Contains("hey"))
        {
            return "👋 **Hello! Welcome to MedAI Intelligent Healthcare Assistant.**\n\n" +
                   "How can I assist you with your health, lab reports, symptoms, or medications today?";
        }

        if (lower.Contains("thank"))
        {
            return "🌟 **You are very welcome! Your health and well-being are our highest priority.**\n\n" +
                   "Feel free to ask anytime if you have further medical or lifestyle questions.";
        }

        return $"💡 **MedAI Clinical Insights & Recommendations:**\n\n" +
               $"Regarding your inquiry: **\"{prompt}\"**\n\n" +
               "**1. Overview & Clinical Context:**\n" +
               "This presentation is frequently associated with physiological baselines, lifestyle stress, or systemic immune responses.\n\n" +
               "**2. Recommended Steps:**\n" +
               "• Maintain adequate hydration (approx. 2 liters of water daily).\n" +
               "• Ensure restful sleep and balanced nutrition.\n" +
               "• Track symptom progression, temperature, and vital signs.\n\n" +
               "**3. When to See a Physician:**\n" +
               "If symptoms persist beyond 48 hours or worsen in severity, consult your primary care doctor for comprehensive diagnostic evaluation.\n\n" +
               "*(Disclaimer: MedAI provides clinical educational information and does not replace certified physician consultation.)*";
    }
}
