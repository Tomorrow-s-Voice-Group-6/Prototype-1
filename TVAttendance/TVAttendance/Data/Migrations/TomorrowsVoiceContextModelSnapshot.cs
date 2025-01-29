﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TVAttendance.Data;

#nullable disable

namespace TVAttendance.Data.Migrations
{
    [DbContext(typeof(TomorrowsVoiceContext))]
    partial class TomorrowsVoiceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

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

                    b.Property<int?>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("CityID");

                    b.HasIndex("DirectorID");

                    b.ToTable("Chapters", (string)null);
                });

            modelBuilder.Entity("TVAttendance.Models.City", b =>
                {
                    b.Property<int>("CityID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CityName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("CityID");

                    b.ToTable("Cities", (string)null);
                });

            modelBuilder.Entity("TVAttendance.Models.Director", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
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
                        .HasColumnType("TEXT");

                    b.Property<bool>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("FirstName", "LastName", "DOB")
                        .IsUnique();

                    b.ToTable("Directors", (string)null);
                });

            modelBuilder.Entity("TVAttendance.Models.Session", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.HasIndex("CityID");

                    b.ToTable("Sessions", (string)null);
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

                    b.Property<int?>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("DOB")
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

                    b.HasIndex("CityID");

                    b.HasIndex("FirstName", "LastName", "DOB")
                        .IsUnique();

                    b.ToTable("Singers", (string)null);
                });

            modelBuilder.Entity("TVAttendance.Models.SingerSession", b =>
                {
                    b.Property<int>("SingerID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SessionID")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Attended")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Notes")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("SingerID", "SessionID");

                    b.HasIndex("SessionID");

                    b.ToTable("SingerSessions", (string)null);
                });

            modelBuilder.Entity("TVAttendance.Models.Volunteer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateOnly>("DOB")
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

                    b.ToTable("Volunteers", (string)null);
                });

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.HasOne("TVAttendance.Models.City", null)
                        .WithMany("Chapters")
                        .HasForeignKey("CityID");

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

                    b.HasOne("TVAttendance.Models.City", null)
                        .WithMany("Programs")
                        .HasForeignKey("CityID");

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.HasOne("TVAttendance.Models.Chapter", "Chapter")
                        .WithMany("Singers")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TVAttendance.Models.City", null)
                        .WithMany("Singers")
                        .HasForeignKey("CityID");

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
                    b.HasOne("TVAttendance.Models.Chapter", "Chapter")
                        .WithMany("Volunteers")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TVAttendance.Models.Chapter", b =>
                {
                    b.Navigation("Sessions");

                    b.Navigation("Singers");

                    b.Navigation("Volunteers");
                });

            modelBuilder.Entity("TVAttendance.Models.City", b =>
                {
                    b.Navigation("Chapters");

                    b.Navigation("Programs");

                    b.Navigation("Singers");
                });

            modelBuilder.Entity("TVAttendance.Models.Session", b =>
                {
                    b.Navigation("SingerSessions");
                });

            modelBuilder.Entity("TVAttendance.Models.Singer", b =>
                {
                    b.Navigation("SingerSessions");
                });
#pragma warning restore 612, 618
        }
    }
}
