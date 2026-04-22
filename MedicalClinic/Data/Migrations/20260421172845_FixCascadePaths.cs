using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalClinic.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Patients_PatientId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalDocument_Patients_PatientId",
                table: "ExternalDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecordEntry_Patients_PatientId",
                table: "MedicalRecordEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Patients_PatientId",
                table: "Review");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Patients_PatientId",
                table: "Appointment",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalDocument_Patients_PatientId",
                table: "ExternalDocument",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecordEntry_Patients_PatientId",
                table: "MedicalRecordEntry",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Patients_PatientId",
                table: "Review",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Patients_PatientId",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalDocument_Patients_PatientId",
                table: "ExternalDocument");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalRecordEntry_Patients_PatientId",
                table: "MedicalRecordEntry");

            migrationBuilder.DropForeignKey(
                name: "FK_Review_Patients_PatientId",
                table: "Review");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Patients_PatientId",
                table: "Appointment",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalDocument_Patients_PatientId",
                table: "ExternalDocument",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalRecordEntry_Patients_PatientId",
                table: "MedicalRecordEntry",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Patients_PatientId",
                table: "Review",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
