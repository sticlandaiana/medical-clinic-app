using Microsoft.AspNetCore.Identity;

namespace MedicalClinic.Models
{
    public class ExternalDocument
    {
        public int ExternalDocumentId { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        public string UserId { get; set; }

        public string FilePath { get; set; }
        public string DocumentType { get; set; }
    }
}
