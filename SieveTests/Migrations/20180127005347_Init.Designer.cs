﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using RzsSieveTests.Entities;
using System;

namespace RzsSieveTests.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180127005347_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SieveTests.Entities.Post", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CommentCount");

                    b.Property<DateTimeOffset>("DateCreated");

                    b.Property<int>("LikeCount");

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Posts");
                });
#pragma warning restore 612, 618
        }
    }
}
