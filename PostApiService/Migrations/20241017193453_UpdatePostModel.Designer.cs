﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace PostApiService.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241017193453_UpdatePostModel")]
    partial class UpdatePostModel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PostApiService.Models.Comment", b =>
                {
                    b.Property<int>("CommentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CommentId"));

                    b.Property<string>("Author")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("PostId")
                        .HasColumnType("int");

                    b.HasKey("CommentId");

                    b.HasIndex("PostId");

                    b.ToTable("Comments");

                    b.HasData(
                        new
                        {
                            CommentId = 1,
                            Author = "John Doe",
                            Content = "Great post!",
                            CreatedAt = new DateTime(2024, 10, 17, 22, 34, 51, 611, DateTimeKind.Local).AddTicks(8842),
                            PostId = 1
                        },
                        new
                        {
                            CommentId = 2,
                            Author = "Jane Doe",
                            Content = "I totally agree with this!",
                            CreatedAt = new DateTime(2024, 10, 17, 22, 34, 51, 611, DateTimeKind.Local).AddTicks(8854),
                            PostId = 1
                        },
                        new
                        {
                            CommentId = 3,
                            Author = "Alice",
                            Content = "This is a comment on the second post.",
                            CreatedAt = new DateTime(2024, 10, 17, 22, 34, 51, 611, DateTimeKind.Local).AddTicks(8862),
                            PostId = 2
                        });
                });

            modelBuilder.Entity("PostApiService.Models.Post", b =>
                {
                    b.Property<int>("PostId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PostId"));

                    b.Property<string>("Author")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MetaDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MetaTitle")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PostId");

                    b.ToTable("Posts");

                    b.HasData(
                        new
                        {
                            PostId = 1,
                            Author = "Peter Jack",
                            Content = "This is the content of the first post.",
                            CreateAt = new DateTime(2024, 10, 17, 22, 34, 51, 611, DateTimeKind.Local).AddTicks(8214),
                            Description = "Description for first post",
                            ImageUrl = "https://ibb.co/3M2k1wQ",
                            MetaDescription = "This is meta description",
                            MetaTitle = "Meta title info",
                            Slug = "http://localhost:4200/first-post",
                            Title = "First Post"
                        },
                        new
                        {
                            PostId = 2,
                            Author = "Jay Way",
                            Content = "This is the content of the second post.",
                            CreateAt = new DateTime(2024, 10, 17, 22, 34, 51, 611, DateTimeKind.Local).AddTicks(8324),
                            Description = "Description for second post",
                            ImageUrl = "https://ibb.co/3M2k1wQ",
                            MetaDescription = "This is meta description 2",
                            MetaTitle = "Meta title info 2",
                            Slug = "http://localhost:4200/second-post",
                            Title = "Second Post"
                        });
                });

            modelBuilder.Entity("PostApiService.Models.Comment", b =>
                {
                    b.HasOne("PostApiService.Models.Post", "Post")
                        .WithMany("Comments")
                        .HasForeignKey("PostId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Post");
                });

            modelBuilder.Entity("PostApiService.Models.Post", b =>
                {
                    b.Navigation("Comments");
                });
#pragma warning restore 612, 618
        }
    }
}
