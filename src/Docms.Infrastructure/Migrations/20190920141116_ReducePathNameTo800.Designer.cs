﻿// <auto-generated />
using System;
using Docms.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Docms.Infrastructure.Migrations
{
    [DbContext(typeof(DocmsContext))]
    [Migration("20190920141116_ReducePathNameTo800")]
    partial class ReducePathNameTo800
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Docms.Domain.Documents.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ContentType");

                    b.Property<DateTime>("Created");

                    b.Property<long>("FileSize");

                    b.Property<string>("Hash");

                    b.Property<DateTime>("LastModified");

                    b.Property<string>("Path")
                        .HasMaxLength(800);

                    b.Property<string>("StorageKey");

                    b.HasKey("Id");

                    b.HasIndex("Path");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Docms.Domain.Identity.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DeviceId");

                    b.Property<string>("DeviceUserAgent");

                    b.Property<bool>("Granted");

                    b.Property<string>("UsedBy");

                    b.HasKey("Id");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("Docms.Infrastructure.Identity.DocmsUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id");

                    b.Property<string>("SecurityStamp")
                        .HasColumnName("SecurityStamp");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Docms.Queries.Blobs.BlobEntry", b =>
                {
                    b.Property<string>("Path")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Path")
                        .HasMaxLength(800);

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("Name")
                        .HasColumnName("Name");

                    b.Property<string>("ParentPath")
                        .HasColumnName("ParentPath")
                        .HasMaxLength(800);

                    b.HasKey("Path");

                    b.HasIndex("ParentPath");

                    b.ToTable("Entries");

                    b.HasDiscriminator<string>("Discriminator").HasValue("BlobEntry");
                });

            modelBuilder.Entity("Docms.Queries.DeviceAuthorization.DeviceGrant", b =>
                {
                    b.Property<string>("DeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("DeviceId");

                    b.Property<string>("DeviceUserAgent")
                        .HasColumnName("DeviceUserAgent");

                    b.Property<DateTime?>("GrantedAt")
                        .HasColumnName("GrantedAt");

                    b.Property<string>("GrantedBy")
                        .HasColumnName("GrantedBy");

                    b.Property<bool>("IsGranted")
                        .HasColumnName("IsGranted");

                    b.Property<DateTime>("LastAccessTime")
                        .HasColumnName("LastAccessTime");

                    b.Property<string>("LastAccessUserId")
                        .HasColumnName("LastAccessUserId");

                    b.Property<string>("LastAccessUserName")
                        .HasColumnName("LastAccessUserName");

                    b.HasKey("DeviceId");

                    b.ToTable("DeviceGrants");
                });

            modelBuilder.Entity("Docms.Queries.DocumentHistories.DocumentHistory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("Id");

                    b.Property<string>("ContentType")
                        .HasColumnName("ContentType");

                    b.Property<DateTime?>("Created")
                        .HasColumnName("Created");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnName("Discriminator");

                    b.Property<int>("DocumentId")
                        .HasColumnName("DocumentId");

                    b.Property<long?>("FileSize")
                        .HasColumnName("FileSize");

                    b.Property<string>("Hash")
                        .HasColumnName("Hash");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnName("LastModified");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnName("Path")
                        .HasMaxLength(800);

                    b.Property<string>("StorageKey")
                        .HasColumnName("StorageKey");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnName("Timestamp");

                    b.HasKey("Id");

                    b.HasIndex("Path", "Timestamp");

                    b.ToTable("DocumentHistories");
                });

            modelBuilder.Entity("Docms.Queries.Blobs.Blob", b =>
                {
                    b.HasBaseType("Docms.Queries.Blobs.BlobEntry");

                    b.Property<string>("ContentType")
                        .HasColumnName("ContentType");

                    b.Property<DateTime>("Created")
                        .HasColumnName("Created");

                    b.Property<int>("DocumentId")
                        .HasColumnName("DocumentId");

                    b.Property<long>("FileSize")
                        .HasColumnName("FileSize");

                    b.Property<string>("Hash")
                        .HasColumnName("Hash");

                    b.Property<DateTime>("LastModified")
                        .HasColumnName("LastModified");

                    b.Property<string>("StorageKey")
                        .HasColumnName("StorageKey");

                    b.HasDiscriminator().HasValue("Blob");
                });

            modelBuilder.Entity("Docms.Queries.Blobs.BlobContainer", b =>
                {
                    b.HasBaseType("Docms.Queries.Blobs.BlobEntry");

                    b.HasDiscriminator().HasValue("BlobContainer");
                });

            modelBuilder.Entity("Docms.Queries.Blobs.BlobEntry", b =>
                {
                    b.HasOne("Docms.Queries.Blobs.BlobContainer")
                        .WithMany("Entries")
                        .HasForeignKey("ParentPath");
                });
#pragma warning restore 612, 618
        }
    }
}
