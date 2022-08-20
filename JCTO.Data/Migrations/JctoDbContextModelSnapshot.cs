﻿// <auto-generated />
using System;
using JCTO.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JCTO.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    partial class JctoDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("JCTO.Domain.Entities.Customer", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ConcurrencyKey")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedById")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Inactive")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("LastUpdatedById")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("LastUpdatedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("LastUpdatedById");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("JCTO.Domain.Entities.Product", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<Guid?>("ConcurrencyKey")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedById")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Inactive")
                        .HasColumnType("boolean");

                    b.Property<Guid?>("LastUpdatedById")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("LastUpdatedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("CreatedById");

                    b.HasIndex("LastUpdatedById");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("JCTO.Domain.Entities.User", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ConcurrencyKey")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedById")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<Guid?>("LastUpdatedById")
                        .IsRequired()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("LastUpdatedDateUtc")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("LastUpdatedById");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("JCTO.Domain.Entities.Customer", b =>
                {
                    b.HasOne("JCTO.Domain.Entities.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("JCTO.Domain.Entities.User", "LastUpdatedBy")
                        .WithMany()
                        .HasForeignKey("LastUpdatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("LastUpdatedBy");
                });

            modelBuilder.Entity("JCTO.Domain.Entities.Product", b =>
                {
                    b.HasOne("JCTO.Domain.Entities.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("JCTO.Domain.Entities.User", "LastUpdatedBy")
                        .WithMany()
                        .HasForeignKey("LastUpdatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("LastUpdatedBy");
                });

            modelBuilder.Entity("JCTO.Domain.Entities.User", b =>
                {
                    b.HasOne("JCTO.Domain.Entities.User", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("JCTO.Domain.Entities.User", "LastUpdatedBy")
                        .WithMany()
                        .HasForeignKey("LastUpdatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");

                    b.Navigation("LastUpdatedBy");
                });
#pragma warning restore 612, 618
        }
    }
}
