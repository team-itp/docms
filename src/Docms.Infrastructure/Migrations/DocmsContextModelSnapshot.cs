﻿// <auto-generated />
using System;
using Docms.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Docms.Infrastructure.Migrations
{
    [DbContext(typeof(DocmsContext))]
    partial class DocmsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Docms.Domain.Clients.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClientId");

                    b.Property<string>("IpAddress");

                    b.Property<bool>("IsAccepted");

                    b.Property<DateTime?>("LastAccessedTime");

                    b.Property<string>("RequestId");

                    b.Property<string>("RequestType");

                    b.Property<int>("Status");

                    b.Property<string>("Type");

                    b.HasKey("Id");

                    b.ToTable("Clients");
                });

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

                    b.Property<string>("StorageKey")
                        .HasMaxLength(800);

                    b.HasKey("Id");

                    b.HasIndex("Path");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("Docms.Domain.Identity.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Deleted");

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

            modelBuilder.Entity("Docms.Infrastructure.Identity.DocmsUserRole", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnName("UserId");

                    b.Property<string>("Role")
                        .HasColumnName("Role");

                    b.HasKey("UserId", "Role");

                    b.ToTable("UserRoles");
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

            modelBuilder.Entity("Docms.Queries.Clients.ClientInfo", b =>
                {
                    b.Property<string>("ClientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("ClientId");

                    b.Property<DateTime?>("AcceptedAt")
                        .HasColumnName("AcceptedAt");

                    b.Property<string>("AcceptedRequestId")
                        .HasColumnName("AcceptedRequestId");

                    b.Property<string>("AcceptedRequestType")
                        .HasColumnName("AcceptedRequestType");

                    b.Property<string>("IpAddress")
                        .HasColumnName("IpAddress");

                    b.Property<DateTime?>("LastAccessedTime")
                        .HasColumnName("LastAccessedTime");

                    b.Property<string>("LastMessage")
                        .HasColumnName("LastMessage");

                    b.Property<string>("RequestId")
                        .HasColumnName("RequestId");

                    b.Property<string>("RequestType")
                        .HasColumnName("RequestType");

                    b.Property<DateTime?>("RequestedAt")
                        .HasColumnName("RequestedAt");

                    b.Property<string>("Status")
                        .HasColumnName("Status");

                    b.Property<string>("Type")
                        .HasColumnName("Type");

                    b.HasKey("ClientId");

                    b.ToTable("ClientInfo");
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

                    b.Property<bool>("IsDeleted")
                        .HasColumnName("IsDeleted");

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
                        .HasColumnName("StorageKey")
                        .HasMaxLength(800);

                    b.Property<DateTime>("Timestamp")
                        .HasColumnName("Timestamp");

                    b.HasKey("Id")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.HasIndex("DocumentId");

                    b.HasIndex("Path");

                    b.HasIndex("StorageKey");

                    b.HasIndex("Timestamp")
                        .HasAnnotation("SqlServer:Clustered", true);

                    b.HasIndex("Path", "Timestamp");

                    b.HasIndex("Timestamp", "Id");

                    b.HasIndex("Timestamp", "Path");

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
                        .HasColumnName("StorageKey")
                        .HasMaxLength(800);

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
