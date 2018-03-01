﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using SFA.DAS.AssessorService.Data;
using SFA.DAS.AssessorService.Domain.Enums;
using System;

namespace SFA.DAS.AssessorService.Data.Migrations
{
    [DbContext(typeof(AssessorDbContext))]
    partial class AssessorDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.Certificate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CertificateData");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<Guid>("CreatedBy");

                    b.Property<DateTime?>("DeletedAt");

                    b.Property<Guid>("DeletedBy");

                    b.Property<Guid>("OrganisationId");

                    b.Property<int>("Status");

                    b.Property<DateTime?>("UpdatedAt");

                    b.Property<Guid>("UpdatedBy");

                    b.HasKey("Id");

                    b.HasIndex("OrganisationId");

                    b.ToTable("Certificates");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.CertificateLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Action");

                    b.Property<Guid>("CertificateId");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime?>("DeletedAt");

                    b.Property<int>("EndPointAssessorCertificateId");

                    b.Property<DateTime>("EventTime");

                    b.Property<int>("Status");

                    b.Property<DateTime?>("UpdatedAt");

                    b.HasKey("Id");

                    b.HasIndex("CertificateId");

                    b.ToTable("CertificateLogs");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.Contact", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ContactStatus");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime?>("DeletedAt");

                    b.Property<string>("DisplayName");

                    b.Property<string>("Email");

                    b.Property<string>("EndPointAssessorOrganisationId");

                    b.Property<Guid>("OrganisationId");

                    b.Property<DateTime?>("UpdatedAt");

                    b.Property<string>("Username")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAlternateKey("Username");

                    b.HasIndex("OrganisationId");

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.Organisation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime?>("DeletedAt");

                    b.Property<string>("EndPointAssessorName");

                    b.Property<string>("EndPointAssessorOrganisationId");

                    b.Property<int>("EndPointAssessorUkprn");

                    b.Property<int>("OrganisationStatus");

                    b.Property<Guid?>("PrimaryContactId");

                    b.Property<DateTime?>("UpdatedAt");

                    b.HasKey("Id");

                    b.ToTable("Organisations");
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.Certificate", b =>
                {
                    b.HasOne("SFA.DAS.AssessorService.Domain.Entities.Organisation", "Organisation")
                        .WithMany("Certificates")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.CertificateLog", b =>
                {
                    b.HasOne("SFA.DAS.AssessorService.Domain.Entities.Certificate", "Certificate")
                        .WithMany()
                        .HasForeignKey("CertificateId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SFA.DAS.AssessorService.Domain.Entities.Contact", b =>
                {
                    b.HasOne("SFA.DAS.AssessorService.Domain.Entities.Organisation", "Organisation")
                        .WithMany("Contacts")
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
