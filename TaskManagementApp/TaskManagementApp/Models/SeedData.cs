using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TaskManagementApp.Models
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // ----------------------------------------------------
                // 0. DEFINIM ID-URILE AICI (vizibile peste tot)
                // ----------------------------------------------------

                var adminId = "8e445865-a24d-4543-a6c6-9443d048cdb0";
                var organizerId = "8e445865-a24d-4543-a6c6-9443d048cdb1";
                var memberId = "8e445865-a24d-4543-a6c6-9443d048cdb2";

                var adminRoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210";
                var memberRoleId = "2c5e174e-3b0e-446f-86af-483d56fd7212";

                // ----------------------------------------------------
                // 1. ROLURI
                // ----------------------------------------------------

                if (!context.Roles.Any())
                {
                    context.Roles.AddRange(
                        new IdentityRole { Id = adminRoleId, Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
                        new IdentityRole { Id = memberRoleId, Name = "Membru", NormalizedName = "MEMBRU" }
                    );
                    context.SaveChanges();
                }

                // ----------------------------------------------------
                // 2. USERI
                // ----------------------------------------------------

                if (!context.Users.Any())
                {
                    var hasher = new PasswordHasher<ApplicationUser>();

                    context.Users.AddRange(
                        new ApplicationUser
                        {
                            Id = adminId,
                            UserName = "admin@task.com",
                            NormalizedUserName = "ADMIN@TASK.COM",
                            Email = "admin@task.com",
                            NormalizedEmail = "ADMIN@TASK.COM",
                            EmailConfirmed = true,
                            FirstName = "Victor",
                            LastName = "Administrator",
                            PasswordHash = hasher.HashPassword(null, "Admin1!")
                        },
                        new ApplicationUser
                        {
                            Id = organizerId,
                            UserName = "organizator@task.com",
                            NormalizedUserName = "ORGANIZATOR@TASK.COM",
                            Email = "organizator@task.com",
                            NormalizedEmail = "ORGANIZATOR@TASK.COM",
                            EmailConfirmed = true,
                            FirstName = "Ana",
                            LastName = "Organizator",
                            PasswordHash = hasher.HashPassword(null, "Organizator1!")
                        },
                        new ApplicationUser
                        {
                            Id = memberId,
                            UserName = "membru@task.com",
                            NormalizedUserName = "MEMBRU@TASK.COM",
                            Email = "membru@task.com",
                            NormalizedEmail = "MEMBRU@TASK.COM",
                            EmailConfirmed = true,
                            FirstName = "George",
                            LastName = "Membru",
                            PasswordHash = hasher.HashPassword(null, "Membru1!")
                        }
                    );

                    context.SaveChanges();
                }

                // ----------------------------------------------------
                // 3. USER-ROLES
                // ----------------------------------------------------

                if (!context.UserRoles.Any())
                {
                    context.UserRoles.AddRange(
                        new IdentityUserRole<string> { UserId = adminId, RoleId = adminRoleId },
                        new IdentityUserRole<string> { UserId = organizerId, RoleId = memberRoleId },
                        new IdentityUserRole<string> { UserId = memberId, RoleId = memberRoleId }
                    );
                    context.SaveChanges();
                }

                // ----------------------------------------------------
                // 4. PROIECTE
                // ----------------------------------------------------

                // Le definim variabilele in afara if-ului ca sa le folosim mai jos
                var p1 = new Project
                {
                    Title = "Dezvoltare Aplicatie Task",
                    Description = "Proiectul de laborator pentru cursul de .NET Core MVC.",
                    OrganizerId = organizerId,
                    Date = DateTime.Now.AddDays(-10)
                };

                var p2 = new Project
                {
                    Title = "Planificare Saptamana Viitoare",
                    Description = "Organizarea sarcinilor pentru echipa de marketing.",
                    OrganizerId = organizerId,
                    Date = DateTime.Now.AddDays(-5)
                };

                var p3 = new Project
                {
                    Title = "Documentatie Interna",
                    Description = "Crearea și actualizarea manualelor de utilizare.",
                    OrganizerId = memberId, // George este organizator aici
                    Date = DateTime.Now.AddDays(-2)
                };

                if (!context.Projects.Any())
                {
                    context.Projects.AddRange(p1, p2, p3);
                    context.SaveChanges(); // Salvam ca sa se genereze ID-urile reale (daca e identity column)
                }
                else
                {
                    // Daca exista deja proiecte, le cautam ca sa avem ID-urile pentru Members si Tasks
                    p1 = context.Projects.First(p => p.Title == "Dezvoltare Aplicatie Task");
                    p3 = context.Projects.First(p => p.Title == "Documentatie Interna");
                }

                // ----------------------------------------------------
                // 5. PROJECT MEMBERS
                // ----------------------------------------------------

                if (!context.ProjectMembers.Any())
                {
                    context.ProjectMembers.AddRange(
                        // P1: Ana e Organizator (Accepted), George e invitat (Pending)
                        new ProjectMember { ProjectId = p1.Id, UserId = organizerId, IsAccepted = true },
                        new ProjectMember { ProjectId = p1.Id, UserId = memberId, IsAccepted = false },

                        // P2: Ana e Organizator
                        new ProjectMember { ProjectId = p2.Id, UserId = organizerId, IsAccepted = true },

                        // P3: George e Organizator - TREBUIE SA FIE TRUE [FIX]
                        new ProjectMember { ProjectId = p3.Id, UserId = memberId, IsAccepted = true }
                    );
                    context.SaveChanges();
                }

                // ----------------------------------------------------
                // 6. TASKS
                // ----------------------------------------------------

                if (!context.Tasks.Any())
                {
                    context.Tasks.AddRange(
                        new Models.Task
                        {
                            Title = "Setup Modele si DB Context",
                            Description = "Definirea tuturor modelelor si relatiilor.",
                            ProjectId = p1.Id,
                            UserId = organizerId,
                            Status = "Completed",
                            StartDate = DateTime.Now.AddDays(-10),
                            EndDate = DateTime.Now.AddDays(-8)
                        },
                        new Models.Task
                        {
                            Title = "Implementare CRUD Proiecte",
                            Description = "Functionalitate CRUD.",
                            ProjectId = p1.Id,
                            UserId = memberId, // George are task, desi e pending (se poate, dar nu vede pana nu accepta)
                            Status = "In Progress",
                            StartDate = DateTime.Now.AddDays(-7),
                            EndDate = DateTime.Now.AddDays(1)
                        }
                    );
                    context.SaveChanges();
                }

                // ----------------------------------------------------
                // 7. NOTIFICATIONS (NOU - Pentru Pending Invites)
                // ----------------------------------------------------
                // George (memberId) este Pending in P1, deci trebuie sa aiba notificare

                if (!context.Notifications.Any())
                {
                    context.Notifications.Add(new Notification
                    {
                        UserId = memberId,          // Destinatar: George
                        SenderId = organizerId,     // Expeditor: Ana
                        Text = $"Te-a invitat să te alături proiectului '{p1.Title}'.",
                        Type = "Invite",
                        RelatedEntityId = p1.Id,    // ID-ul proiectului pentru butonul Accept
                        CreatedDate = DateTime.Now,
                        IsRead = false
                    });

                    context.SaveChanges();
                }
            }
        }
    }
}