using Microsoft.AspNetCore.Identity;

namespace MedicalClinic.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            // Creează rolurile
            string[] roles = { "Administrator", "Doctor", "Nurse", "Patient" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Creează contul de admin implicit
            string adminEmail = "admin@medicalclinic.com";
            string adminPassword = "Admin@12345";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }
    }
}
