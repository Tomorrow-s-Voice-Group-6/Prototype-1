﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TVAttendance.Data;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    [DbContext(typeof(TomorrowsVoiceContext))]
    [Migration("20250120225827_TVContextConstraints")]
    partial class TVContextConstraints
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int?>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ProgramID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ProgramID1")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("DirectorID");

                    b.HasIndex("ProgramID1");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("TVAttendance.Models.Director", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DOB")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("HireDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("FirstName", "LastName", "DOB")
                        .IsUnique();

                    b.ToTable("Directors");
                });

            modelBuilder.Entity("TVAttendance.Models.Program", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.ToTable("Programs");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DOB")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EmergencyContactFirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("EmergencyContactLastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("EmergencyContactPhone")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RegisterDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.HasIndex("FirstName", "LastName", "DOB")
                        .IsUnique();

                    b.ToTable("Singers");
                });

            modelBuilder.Entity("TVAttendance.Models.SingerProgram", b =>
                {
                    b.Property<int>("SingerID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ProgramID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("SingerID", "ProgramID");

                    b.HasIndex("ProgramID");

                    b.ToTable("SingerPrograms");
                });

            modelBuilder.Entity("TVAttendance.Models.Volunteer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DOB")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(55)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("RegisterDate")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.ToTable("Volunteers");
                });

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.HasOne("TVAttendance.Models.Director", "Director")
                        .WithMany()
                        .HasForeignKey("DirectorID");

                    b.HasOne("TVAttendance.Models.Program", "Program")
                        .WithMany()
                        .HasForeignKey("ProgramID1");

                    b.Navigation("Director");

                    b.Navigation("Program");
                });

            modelBuilder.Entity("TVAttendance.Models.Program", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", "Chapter")
                        .WithMany("Programs")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", "chapter")
                        .WithMany("Singers")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.SingerProgram", b =>
                {
                    b.HasOne("TVAttendance.Models.Program", "Program")
                        .WithMany("SingerPrograms")
                        .HasForeignKey("ProgramID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TVAttendance.Models.Singer", "Singer")
                        .WithMany("SingerPrograms")
                        .HasForeignKey("SingerID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Program");

                    b.Navigation("Singer");
                });

            modelBuilder.Entity("TVAttendance.Models.Volunteer", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", "chapter")
                        .WithMany("Volunteers")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.Navigation("Programs");

                    b.Navigation("Singers");

                    b.Navigation("Volunteers");
                });

            modelBuilder.Entity("TVAttendance.Models.Program", b =>
                {
                    b.Navigation("SingerPrograms");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.Navigation("SingerPrograms");
                });
#pragma warning restore 612, 618
        }
    }
}
