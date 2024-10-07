﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Data;


/// <summary>
/// Razred koji predstavlja kontekst baze podataka.
/// </summary>
public partial class RPPP06Context : DbContext
{

    /// <summary>
    /// Konstruktor razreda.
    /// </summary>
    /// <param name="options">Opcije konteksta baze podataka.</param>
    public RPPP06Context(DbContextOptions<RPPP06Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Aktivnost> Aktivnost { get; set; }

    public virtual DbSet<Dokumentacija> Dokumentacija { get; set; }

    public virtual DbSet<Etapa> Etapa { get; set; }

    public virtual DbSet<PlanProjekta> PlanProjekta { get; set; }

    public virtual DbSet<Posao> Posao { get; set; }

    public virtual DbSet<Prioritet> Prioritet { get; set; }

    public virtual DbSet<Projekt> Projekt { get; set; }

    public virtual DbSet<ProjektnaKartica> ProjektnaKartica { get; set; }

    public virtual DbSet<Status> Status { get; set; }

    public virtual DbSet<Suradnik> Suradnik { get; set; }

    public virtual DbSet<TipZahtjeva> TipZahtjeva { get; set; }

    public virtual DbSet<Transakcija> Transakcija { get; set; }

    public virtual DbSet<Uloga> Uloga { get; set; }

    public virtual DbSet<VrstaDokumentacije> VrstaDokumentacije { get; set; }

    public virtual DbSet<VrstaPosla> VrstaPosla { get; set; }

    public virtual DbSet<VrstaProjekta> VrstaProjekta { get; set; }

    public virtual DbSet<VrstaTransakcije> VrstaTransakcije { get; set; }

    public virtual DbSet<Zadatak> Zadatak { get; set; }

    public virtual DbSet<Zahtjev> Zahtjev { get; set; }

    /// <summary>
    /// Metoda koja se poziva pri kreiranju modela, a koja služi za konfiguraciju modela prema bazi podataka.
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Aktivnost>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<Dokumentacija>(entity =>
        {
            entity.ToTable("Dokumentacija");

            entity.HasIndex(e => e.ProjektId, "IX_Dokumentacija_ProjektId");

            entity.HasIndex(e => e.VrstaDokumentacijeId, "IX_Dokumentacija_VrstaDokumentacijeId");

            entity.Property(e => e.NazivDokumentacije).IsRequired();
            entity.Property(e => e.URL)
                .IsRequired()
                .HasColumnName("URL");

            entity.HasOne(d => d.Projekt).WithMany(p => p.Dokumentacija).HasForeignKey(d => d.ProjektId);

            entity.HasOne(d => d.VrstaDokumentacije).WithMany(p => p.Dokumentacija ).HasForeignKey(d => d.VrstaDokumentacijeId);
        });

        modelBuilder.Entity<Etapa>(entity =>
        {
            entity.HasIndex(e => e.AktivnostId, "IX_Etapa_AktivnostId");

            entity.HasIndex(e => e.PlanProjektaId, "IX_Etapa_PlanProjektaId");

            entity.Property(e => e.Ime).IsRequired();

            entity.HasOne(d => d.Aktivnost).WithMany(p => p.Etapa).HasForeignKey(d => d.AktivnostId);

            entity.HasOne(d => d.PlanProjekta).WithMany(p => p.Etapa).HasForeignKey(d => d.PlanProjektaId);
        });

        modelBuilder.Entity<PlanProjekta>(entity =>
        {
            entity.HasIndex(e => e.ProjektId, "IX_PlanProjekta_ProjektId").IsUnique();

            entity.HasIndex(e => e.VoditeljEmail, "IX_PlanProjekta_VoditeljEmail");

            entity.Property(e => e.VoditeljEmail).IsRequired();

            entity.HasOne(d => d.Projekt).WithOne(p => p.PlanProjekta).HasForeignKey<PlanProjekta>(d => d.ProjektId);

            entity.HasOne(d => d.VoditeljEmailNavigation).WithMany(p => p.PlanProjekta).HasForeignKey(d => d.VoditeljEmail);
        });

        modelBuilder.Entity<Posao>(entity =>
        {
            entity.HasIndex(e => e.SuradnikEmail, "IX_Posao_SuradnikEmail");

            entity.HasIndex(e => e.VrstaPoslaId, "IX_Posao_VrstaPoslaId");

            entity.HasIndex(e => e.ZadatakId, "IX_Posao_ZadatakId");

            entity.Property(e => e.SuradnikEmail).IsRequired();

            entity.HasOne(d => d.SuradnikEmailNavigation).WithMany(p => p.Posao)
                .HasForeignKey(d => d.SuradnikEmail)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.VrstaPosla).WithMany(p => p.Posao).HasForeignKey(d => d.VrstaPoslaId);

            entity.HasOne(d => d.Zadatak).WithMany(p => p.Posao)
                .HasForeignKey(d => d.ZadatakId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Prioritet>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<Projekt>(entity =>
        {
            entity.HasIndex(e => e.VrstaProjektaId, "IX_Projekt_VrstaProjektaId");

            entity.Property(e => e.Ime).IsRequired();

            entity.HasOne(d => d.VrstaProjekta).WithMany(p => p.Projekt).HasForeignKey(d => d.VrstaProjektaId);
        });

        modelBuilder.Entity<ProjektnaKartica>(entity =>
        {
            entity.HasIndex(e => e.ProjektId, "IX_ProjektnaKartica_ProjektId");

            entity.Property(e => e.Banka).IsRequired();

            entity.HasOne(d => d.Projekt).WithMany(p => p.ProjektnaKartica).HasForeignKey(d => d.ProjektId);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
            entity.Property(e => e.ZastavicaAktivnosti).IsRequired();
        });

        modelBuilder.Entity<Suradnik>(entity =>
        {
            entity.HasKey(e => e.Email);

            entity.HasIndex(e => e.NadređeniEmail, "IX_Suradnik_NadređeniEmail");

            entity.Property(e => e.Ime).IsRequired();
            entity.Property(e => e.MjestoStanovanja).IsRequired();
            entity.Property(e => e.Prezime).IsRequired();

            entity.HasOne(d => d.NadređeniEmailNavigation).WithMany(p => p.InverseNadređeniEmailNavigation).HasForeignKey(d => d.NadređeniEmail);

            entity.HasMany(d => d.Projekt).WithMany(p => p.SuradnikEmail)
                .UsingEntity<Dictionary<string, object>>(
                    "SuradnikProjekt",
                    r => r.HasOne<Projekt>().WithMany().HasForeignKey("ProjektId"),
                    l => l.HasOne<Suradnik>().WithMany().HasForeignKey("SuradnikEmail"),
                    j =>
                    {
                        j.HasKey("SuradnikEmail", "ProjektId");
                        j.HasIndex(new[] { "ProjektId" }, "IX_SuradnikProjekt_ProjektId");
                    });

            entity.HasMany(d => d.Uloga).WithMany(p => p.SuradnikEmail)
                .UsingEntity<Dictionary<string, object>>(
                    "SuradnikUloga",
                    r => r.HasOne<Uloga>().WithMany().HasForeignKey("UlogaId"),
                    l => l.HasOne<Suradnik>().WithMany().HasForeignKey("SuradnikEmail"),
                    j =>
                    {
                        j.HasKey("SuradnikEmail", "UlogaId");
                        j.HasIndex(new[] { "UlogaId" }, "IX_SuradnikUloga_UlogaId");
                    });
        });

        modelBuilder.Entity<TipZahtjeva>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<Transakcija>(entity =>
        {
            entity.HasIndex(e => e.ProjektnaKarticaIsporučiteljId, "IX_Transakcija_ProjektnaKarticaIsporučiteljId");

            entity.HasIndex(e => e.ProjektnaKarticaPrimateljId, "IX_Transakcija_ProjektnaKarticaPrimateljId");

            entity.HasIndex(e => e.VrstaTransakcijeId, "IX_Transakcija_VrstaTransakcijeId");

            entity.HasOne(d => d.ProjektnaKarticaIsporučitelj).WithMany(p => p.TransakcijaProjektnaKarticaIsporučitelj).HasForeignKey(d => d.ProjektnaKarticaIsporučiteljId);

            entity.HasOne(d => d.ProjektnaKarticaPrimatelj).WithMany(p => p.TransakcijaProjektnaKarticaPrimatelj).HasForeignKey(d => d.ProjektnaKarticaPrimateljId);

            entity.HasOne(d => d.VrstaTransakcije).WithMany(p => p.Transakcija).HasForeignKey(d => d.VrstaTransakcijeId);
        });

        modelBuilder.Entity<Uloga>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<VrstaDokumentacije>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<VrstaPosla>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<VrstaProjekta>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<VrstaTransakcije>(entity =>
        {
            entity.Property(e => e.Ime).IsRequired();
        });

        modelBuilder.Entity<Zadatak>(entity =>
        {
            entity.HasIndex(e => e.NositeljEmail, "IX_Zadatak_NositeljEmail");

            entity.HasIndex(e => e.StatusId, "IX_Zadatak_StatusId");

            entity.HasIndex(e => e.ZahtjevId, "IX_Zadatak_ZahtjevId");

            entity.HasOne(d => d.NositeljEmailNavigation).WithMany(p => p.Zadatak).HasForeignKey(d => d.NositeljEmail);

            entity.HasOne(d => d.Status).WithMany(p => p.Zadatak).HasForeignKey(d => d.StatusId);

            entity.HasOne(d => d.Zahtjev).WithMany(p => p.Zadatak).HasForeignKey(d => d.ZahtjevId);
        });

        modelBuilder.Entity<Zahtjev>(entity =>
        {
            entity.HasIndex(e => e.PlanProjektaId, "IX_Zahtjev_PlanProjektaId");

            entity.HasIndex(e => e.PrioritetId, "IX_Zahtjev_PrioritetId");

            entity.HasIndex(e => e.TipZahtjevaId, "IX_Zahtjev_TipZahtjevaId");

            entity.Property(e => e.Ime).IsRequired();

            entity.HasOne(d => d.PlanProjekta).WithMany(p => p.Zahtjev).HasForeignKey(d => d.PlanProjektaId);

            entity.HasOne(d => d.Prioritet).WithMany(p => p.Zahtjev).HasForeignKey(d => d.PrioritetId);

            entity.HasOne(d => d.TipZahtjeva).WithMany(p => p.Zahtjev).HasForeignKey(d => d.TipZahtjevaId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}