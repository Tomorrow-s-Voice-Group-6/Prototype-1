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
    [Migration("20250213230046_UpdateChapterZipCode")]
    partial class UpdateChapterZipCode
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Province")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("DirectorID");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("TVAttendance.Models.Director", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DOB")
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
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("FirstName", "LastName", "DOB")
                        .IsUnique();

                    b.ToTable("Directors");
                });

            modelBuilder.Entity("TVAttendance.Models.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("EventCity")
                        .IsRequired()
                        .HasMaxLength(35)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EventEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("EventPostalCode")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("TEXT");

                    b.Property<int>("EventProvince")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EventStart")
                        .HasColumnType("TEXT");

                    b.Property<string>("EventStreet")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("EventName", "EventStreet")
                        .IsUnique();

                    b.ToTable("Events");
                });

            modelBuilder.Entity("TVAttendance.Models.Session", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(35)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DOB")
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
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("TEXT");

                    b.Property<int>("Province")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("RegisterDate")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.HasIndex("FirstName", "LastName", "DOB")
                        .IsUnique();

                    b.ToTable("Singers");
                });

            modelBuilder.Entity("TVAttendance.Models.SingerSession", b =>
                {
                    b.Property<int>("SingerID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SessionID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("SingerID", "SessionID");

                    b.HasIndex("SessionID");

                    b.ToTable("SingerSessions");
                });

            modelBuilder.Entity("TVAttendance.Models.Volunteer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChapterID")
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

                    b.Property<DateTime>("RegisterDate")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.ToTable("Volunteers");
                });

            modelBuilder.Entity("TVAttendance.Models.VolunteerEvent", b =>
                {
                    b.Property<int>("EventID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("VolunteerID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NonAttendanceNote")
                        .HasColumnType("TEXT");

                    b.Property<bool>("ShiftAttended")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ShiftEnd")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ShiftStart")
                        .HasColumnType("TEXT");

                    b.HasKey("EventID", "VolunteerID");

                    b.HasIndex("VolunteerID");

                    b.ToTable("VolunteerEvents");
                });

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.HasOne("TVAttendance.Models.Director", "Director")
                        .WithMany()
                        .HasForeignKey("DirectorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Director");
                });

            modelBuilder.Entity("TVAttendance.Models.Session", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", "Chapter")
                        .WithMany("Sessions")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", "Chapter")
                        .WithMany("Singers")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.SingerSession", b =>
                {
                    b.HasOne("TVAttendance.Models.Session", "Session")
                        .WithMany("SingerSessions")
                        .HasForeignKey("SessionID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("TVAttendance.Models.Singer", "Singer")
                        .WithMany("SingerSessions")
                        .HasForeignKey("SingerID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Session");

                    b.Navigation("Singer");
                });

            modelBuilder.Entity("TVAttendance.Models.Volunteer", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", null)
                        .WithMany("Volunteers")
                        .HasForeignKey("ChapterID");
                });

            modelBuilder.Entity("TVAttendance.Models.VolunteerEvent", b =>
                {
                    b.HasOne("TVAttendance.Models.Event", "Event")
                        .WithMany("VolunteerEvents")
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TVAttendance.Models.Volunteer", "Volunteer")
                        .WithMany("VolunteerEvents")
                        .HasForeignKey("VolunteerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");

                    b.Navigation("Volunteer");
                });

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.Navigation("Sessions");

                    b.Navigation("Singers");

                    b.Navigation("Volunteers");
                });

            modelBuilder.Entity("TVAttendance.Models.Event", b =>
                {
                    b.Navigation("VolunteerEvents");
                });

            modelBuilder.Entity("TVAttendance.Models.Session", b =>
                {
                    b.Navigation("SingerSessions");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.Navigation("SingerSessions");
                });

            modelBuilder.Entity("TVAttendance.Models.Volunteer", b =>
                {
                    b.Navigation("VolunteerEvents");
                });
#pragma warning restore 612, 618
        }
    }
}
