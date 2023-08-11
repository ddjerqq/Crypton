﻿// <auto-generated />
using System;
using Crypton.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Crypton.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("00000000000000_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0-preview.7.23375.4");

            modelBuilder.Entity("Crypton.Domain.Entities.Item", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<Guid>("ItemTypeId")
                        .HasColumnType("TEXT")
                        .HasColumnName("item_type_id");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("owner_id");

                    b.HasKey("Id")
                        .HasName("pk_item");

                    b.HasIndex("ItemTypeId")
                        .HasDatabaseName("ix_item_item_type_id");

                    b.HasIndex("OwnerId")
                        .HasDatabaseName("ix_item_owner_id");

                    b.ToTable("item");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<long>("Index")
                        .HasColumnType("INTEGER")
                        .HasColumnName("index");

                    b.Property<long>("Nonce")
                        .HasColumnType("INTEGER")
                        .HasColumnName("nonce");

                    b.Property<string>("PreviousHash")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("previous_hash");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT")
                        .HasColumnName("timestamp");

                    b.Property<string>("discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT")
                        .HasColumnName("discriminator");

                    b.HasKey("Id")
                        .HasName("pk_transactions");

                    b.HasIndex("Index")
                        .IsUnique()
                        .HasDatabaseName("ix_transactions_index");

                    b.ToTable("transaction");

                    b.HasDiscriminator<string>("discriminator").HasValue("Transaction");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Crypton.Domain.Entities.TransactionUser", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .HasColumnType("TEXT")
                        .HasColumnName("transaction_id");

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("user_id");

                    b.Property<bool>("IsSender")
                        .HasColumnType("INTEGER")
                        .HasColumnName("is_sender");

                    b.HasKey("TransactionId", "UserId", "IsSender")
                        .HasName("pk_transaction_user");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_transaction_user_user_id");

                    b.ToTable("transaction_user");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER")
                        .HasColumnName("access_failed_count");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT")
                        .HasColumnName("concurrency_stamp");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_by");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT")
                        .HasColumnName("email");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER")
                        .HasColumnName("email_confirmed");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified_by");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER")
                        .HasColumnName("lockout_enabled");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT")
                        .HasColumnName("lockout_end");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT")
                        .HasColumnName("normalized_email");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT")
                        .HasColumnName("normalized_user_name");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT")
                        .HasColumnName("password_hash");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT")
                        .HasColumnName("phone_number");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER")
                        .HasColumnName("phone_number_confirmed");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT")
                        .HasColumnName("security_stamp");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER")
                        .HasColumnName("two_factor_enabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT")
                        .HasColumnName("user_name");

                    b.HasKey("Id")
                        .HasName("pk_user");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("email_index");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("user_name_index");

                    b.HasIndex("UserName")
                        .IsUnique()
                        .HasDatabaseName("ix_user_user_name");

                    b.ToTable("user", (string)null);
                });

            modelBuilder.Entity("Crypton.Domain.ValueTypes.ItemType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT")
                        .HasColumnName("created");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("created_by");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified");

                    b.Property<string>("LastModifiedBy")
                        .HasColumnType("TEXT")
                        .HasColumnName("last_modified_by");

                    b.Property<float>("MaxRarity")
                        .HasColumnType("REAL")
                        .HasColumnName("max_rarity");

                    b.Property<float>("MinRarity")
                        .HasColumnType("REAL")
                        .HasColumnName("min_rarity");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT")
                        .HasColumnName("price");

                    b.HasKey("Id")
                        .HasName("pk_item_type");

                    b.HasIndex("Id")
                        .IsUnique()
                        .HasDatabaseName("ix_item_type_id");

                    b.HasIndex("Name")
                        .IsUnique()
                        .HasDatabaseName("ix_item_type_name");

                    b.ToTable("item_type");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasColumnName("id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT")
                        .HasColumnName("concurrency_stamp");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT")
                        .HasColumnName("normalized_name");

                    b.HasKey("Id")
                        .HasName("pk_role");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("role_name_index");

                    b.ToTable("role", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT")
                        .HasColumnName("claim_value");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("role_id");

                    b.HasKey("Id")
                        .HasName("pk_role_claim");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_role_claim_role_id");

                    b.ToTable("role_claim", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT")
                        .HasColumnName("claim_type");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT")
                        .HasColumnName("claim_value");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_user_claim");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_user_claim_user_id");

                    b.ToTable("user_claim", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT")
                        .HasColumnName("login_provider");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT")
                        .HasColumnName("provider_key");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT")
                        .HasColumnName("provider_display_name");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("user_id");

                    b.HasKey("LoginProvider", "ProviderKey")
                        .HasName("pk_user_login");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_user_login_user_id");

                    b.ToTable("user_login", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("user_id");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT")
                        .HasColumnName("role_id");

                    b.HasKey("UserId", "RoleId")
                        .HasName("pk_user_role");

                    b.HasIndex("RoleId")
                        .HasDatabaseName("ix_user_role_role_id");

                    b.ToTable("user_role", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT")
                        .HasColumnName("user_id");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT")
                        .HasColumnName("login_provider");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasColumnName("name");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT")
                        .HasColumnName("value");

                    b.HasKey("UserId", "LoginProvider", "Name")
                        .HasName("pk_user_token");

                    b.ToTable("user_token", (string)null);
                });

            modelBuilder.Entity("Crypton.Domain.Entities.BalanceTransaction", b =>
                {
                    b.HasBaseType("Crypton.Domain.Entities.Transaction");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT")
                        .HasColumnName("amount");

                    b.ToTable("transaction");

                    b.HasDiscriminator().HasValue("BalanceTransaction");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.ItemTransaction", b =>
                {
                    b.HasBaseType("Crypton.Domain.Entities.Transaction");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("TEXT")
                        .HasColumnName("item_id");

                    b.HasIndex("ItemId")
                        .HasDatabaseName("ix_transaction_item_id");

                    b.ToTable("transaction");

                    b.HasDiscriminator().HasValue("ItemTransaction");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.Item", b =>
                {
                    b.HasOne("Crypton.Domain.ValueTypes.ItemType", "ItemType")
                        .WithMany()
                        .HasForeignKey("ItemTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_item_item_types_item_type_id");

                    b.HasOne("Crypton.Domain.Entities.User", "Owner")
                        .WithMany("Items")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_item_asp_net_users_owner_id");

                    b.Navigation("ItemType");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.TransactionUser", b =>
                {
                    b.HasOne("Crypton.Domain.Entities.Transaction", "Transaction")
                        .WithMany("Participants")
                        .HasForeignKey("TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_transaction_user_transaction_transaction_id");

                    b.HasOne("Crypton.Domain.Entities.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_transaction_user_asp_net_users_user_id");

                    b.Navigation("Transaction");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_role_claim_role_role_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Crypton.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_claim_user_user_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Crypton.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_login_user_user_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_role_role_role_id");

                    b.HasOne("Crypton.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_role_user_user_id");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Crypton.Domain.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_user_token_user_user_id");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.ItemTransaction", b =>
                {
                    b.HasOne("Crypton.Domain.Entities.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_transaction_item_item_id");

                    b.Navigation("Item");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.Transaction", b =>
                {
                    b.Navigation("Participants");
                });

            modelBuilder.Entity("Crypton.Domain.Entities.User", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
