using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sistema_Hospital.Models;
using Sistema_Hospital.Models.ViewModels;

namespace Sistema_Hospital.Data;

public partial class HospitalContext : DbContext
{
    public HospitalContext()
    {
    }

    public HospitalContext(DbContextOptions<HospitalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Sistema_Hospital.Models.ViewModels.MedicoListaVM> VwMedicosActivos { get; set; }
    public virtual DbSet<Sistema_Hospital.Models.ViewModels.EnfermeroListaVM> VwEnfermerosActivos { get; set; }
    public virtual DbSet<Sistema_Hospital.Models.ViewModels.PacienteListaVM> VwPacientes {  get; set; }

    public virtual DbSet<AreaHospital> AreaHospital { get; set; }

    public virtual DbSet<Bitacora> Bitacora { get; set; }

    public virtual DbSet<Cita> Cita { get; set; }

    public virtual DbSet<Diagnostico> Diagnosticos { get; set; }

    public virtual DbSet<Enfermero> Enfermeros { get; set; }

    public virtual DbSet<Especialidad> Especialidades { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Genero> Generos { get; set; }

    public virtual DbSet<Ingreso> Ingresos { get; set; }

    public virtual DbSet<Medicamento> Medicamentos { get; set; }

    public virtual DbSet<Medico> Medicos { get; set; }

    public virtual DbSet<Paciente> Pacientes { get; set; }

    public virtual DbSet<Prescripcion> Prescripciones { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<UsuarioSistema> UsuarioSistemas { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Name=DefaultConnection");
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AreaHospital>(entity =>
        {
            entity.HasKey(e => e.IdArea).HasName("PK__Area_Hos__42A5C44C08AB7D7D");

            entity.ToTable("Area_Hospital", "Administracion");

            entity.Property(e => e.IdArea).HasColumnName("ID_Area");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NombreArea)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Nombre_Area");

        });

        // Mapeo de la Vista de Seguridad
        modelBuilder.Entity<Sistema_Hospital.Models.ViewModels.MedicoListaVM>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("VW_Medicos_Activos", "Hospitalario");

            entity.Property(e => e.IdMedico).HasColumnName("IdMedico");
            entity.Property(e => e.Nombre).HasColumnName("Nombre");
            entity.Property(e => e.Apellido).HasColumnName("Apellido");
            entity.Property(e => e.DPI).HasColumnName("DPI");
            entity.Property(e => e.Colegiado).HasColumnName("Colegiado");
            entity.Property(e => e.Especialidad).HasColumnName("Especialidad");
        });

        modelBuilder.Entity<Sistema_Hospital.Models.ViewModels.EnfermeroListaVM>(entity =>
        {
            entity.HasNoKey(); // Las vistas no tienen clave primaria física
            entity.ToView("VW_Enfermeros_Activos", "Hospitalario"); // Nombre exacto de tu vista en SQL Server

            // Mapeo de las columnas de la vista con las propiedades del ViewModel
            entity.Property(e => e.IdEnfermero).HasColumnName("IdEnfermero");
            entity.Property(e => e.Nombre).HasColumnName("Nombre");
            entity.Property(e => e.Apellido).HasColumnName("Apellido");
            entity.Property(e => e.Telefono).HasColumnName("Telefono");
            entity.Property(e => e.Correo).HasColumnName("Correo");
            entity.Property(e => e.Genero).HasColumnName("Genero");
        });

        modelBuilder.Entity<PacienteListaVM>(entity =>
        {
            // 1. Indicar que es una entidad sin clave primaria
            entity.HasNoKey();

            // 2. Mapear explícitamente al esquema y nombre de la Vista en SQL
            entity.ToView("VW_Paciente", "Hospitalario");

            // 3. Mapeo de propiedades si los nombres de la DB usan guiones bajos

            entity.Property(e => e.IdPaciente);
            entity.Property(e => e.Nombre);
            entity.Property(e => e.Apellido);
            entity.Property(e => e.FechaNacimiento);
            entity.Property(e => e.Telefono);
            entity.Property(e => e.Correo);
            entity.Property(e => e.Genero);

        });

        modelBuilder.Entity<Bitacora>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PK__Bitacora__2DBF3395849DCFDE");

            entity.ToTable("Bitacora", "Administracion");

            entity.Property(e => e.IdLog).HasColumnName("ID_Log");
            entity.Property(e => e.Accion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FechaHora)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Hora");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Bitacoras)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bitacora_Usuario");
        });

        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(e => e.IdCita).HasName("PK__Cita__7C17FD1666984A66");

            entity.ToTable("Cita", "Hospitalario");

            entity.Property(e => e.IdCita).HasColumnName("ID_Cita");
            entity.Property(e => e.FechaHora).HasColumnType("datetime");
            entity.Property(e => e.IdEstado).HasColumnName("ID_Estado");
            entity.Property(e => e.IdMedico).HasColumnName("ID_Medico");
            entity.Property(e => e.IdPaciente).HasColumnName("ID_Paciente");
            entity.Property(e => e.Motivo)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cita_Estado");

            entity.HasOne(d => d.IdMedicoNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdMedico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cita_Medico");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdPaciente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cita_Paciente");
        });

        modelBuilder.Entity<Diagnostico>(entity =>
        {
            entity.HasKey(e => e.IdDiagnostico).HasName("PK__Diagnost__1A01BCE3EE3C3379");

            entity.ToTable("Diagnostico", "Hospitalario");

            entity.Property(e => e.IdDiagnostico).HasColumnName("ID_Diagnostico");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.IdCita).HasColumnName("ID_Cita");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Diagnosticos)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Diagnostico_Cita");
        });

        modelBuilder.Entity<Enfermero>(entity =>
        {
            entity.HasKey(e => e.IdEnfermero).HasName("PK__Enfermer__82E641DAD2C6F9DA");

            entity.ToTable("Enfermero", "Hospitalario");

            entity.HasIndex(e => e.Dpi, "UQ_Enfermero_DPI").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "UQ__Enfermer__DE4431C41C64CA55").IsUnique();

            entity.Property(e => e.IdEnfermero).HasColumnName("ID_Enfermero");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Dpi)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("DPI");
            entity.Property(e => e.IdGenero).HasColumnName("ID_Genero");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .IsUnicode(false);

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Enfermeros)
                .HasForeignKey(d => d.IdGenero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enfermero_Genero");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Enfermero)
                .HasForeignKey<Enfermero>(d => d.IdUsuario)
                .HasConstraintName("FK_Enfermero_Usuario");
        });

        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.HasKey(e => e.IdEspecialidad).HasName("PK__Especial__5D7732D7064D0DB5");

            entity.ToTable("Especialidad", "Administracion");

            entity.Property(e => e.IdEspecialidad).HasColumnName("ID_Especialidad");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.IdEstado).HasName("PK__Estado__9CF4939561243931");

            entity.ToTable("Estado", "Hospitalario");

            entity.Property(e => e.IdEstado).HasColumnName("ID_Estado");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NombreEstado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Nombre_Estado");
        });

        modelBuilder.Entity<Genero>(entity =>
        {
            entity.HasKey(e => e.IdGenero).HasName("PK__Genero__52F05F3DD41E1566");

            entity.ToTable("Genero", "Administracion");

            entity.Property(e => e.IdGenero).HasColumnName("ID_Genero");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.HasKey(e => e.IdIngreso).HasName("PK__Ingreso__2B7A7D35B2726A2F");

            entity.ToTable("Ingreso", "Hospitalario");

            entity.Property(e => e.IdIngreso).HasColumnName("ID_Ingreso");
            entity.Property(e => e.FechaIngreso)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Ingreso");
            entity.Property(e => e.FechaSalida)
                .HasColumnType("datetime")
                .HasColumnName("Fecha_Salida");
            entity.Property(e => e.IdArea).HasColumnName("ID_Area");
            entity.Property(e => e.IdEstado).HasColumnName("ID_Estado");
            entity.Property(e => e.IdPaciente).HasColumnName("ID_Paciente");
            entity.Property(e => e.Motivo)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.IdAreaNavigation).WithMany(p => p.Ingresos)
                .HasForeignKey(d => d.IdArea)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingreso_Area");

            entity.HasOne(d => d.IdEstadoNavigation).WithMany(p => p.Ingresos)
                .HasForeignKey(d => d.IdEstado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingreso_Estado");

            entity.HasOne(d => d.IdPacienteNavigation).WithMany(p => p.Ingresos)
                .HasForeignKey(d => d.IdPaciente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ingreso_Paciente");
        });

        modelBuilder.Entity<Medicamento>(entity =>
        {
            entity.HasKey(e => e.IdMedicamento).HasName("PK__Medicame__C1C5A0423888573F");

            entity.ToTable("Medicamento", "Hospitalario");

            entity.Property(e => e.IdMedicamento).HasColumnName("ID_Medicamento");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.HasKey(e => e.IdMedico).HasName("PK__Medico__EFBF88F71EA8BEEC");

            entity.ToTable("Medico", "Hospitalario");

            entity.HasIndex(e => e.Dpi, "UQ_Medico_DPI").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "UQ__Medico__DE4431C47F40EB90").IsUnique();

            entity.Property(e => e.IdMedico).HasColumnName("ID_Medico");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Colegiado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Dpi)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("DPI");
            entity.Property(e => e.FechaNacimiento).HasColumnName("Fecha_Nacimiento");
            entity.Property(e => e.IdEspecialidad).HasColumnName("ID_Especialidad");
            entity.Property(e => e.IdGenero).HasColumnName("ID_Genero");
            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .IsUnicode(false);

            entity.HasOne(d => d.IdEspecialidadNavigation).WithMany(p => p.Medicos)
                .HasForeignKey(d => d.IdEspecialidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Medico_Especialidad");

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Medicos)
                .HasForeignKey(d => d.IdGenero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Medico_Genero");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Medico)
                .HasForeignKey<Medico>(d => d.IdUsuario)
                .HasConstraintName("FK_Medico_Usuario");
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.HasKey(e => e.IdPaciente).HasName("PK__Paciente__5F3650616A9870AB");

            entity.ToTable("Paciente", "Hospitalario");

            entity.HasIndex(e => e.Dpi, "UQ_Paciente_DPI").IsUnique();

            entity.Property(e => e.IdPaciente).HasColumnName("ID_Paciente");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Dpi)
                .HasMaxLength(13)
                .IsUnicode(false)
                .HasColumnName("DPI");
            entity.Property(e => e.FechaNacimiento).HasColumnName("Fecha_Nacimiento");
            entity.Property(e => e.IdGenero).HasColumnName("ID_Genero");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(8)
                .IsUnicode(false);

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Pacientes)
                .HasForeignKey(d => d.IdGenero)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Paciente_Genero");
        });

        modelBuilder.Entity<Prescripcion>(entity =>
        {
            entity.HasKey(e => e.IdPrescripcion).HasName("PK__Prescrip__E8394E824566B6C2");

            entity.ToTable("Prescripcion", "Hospitalario");

            entity.Property(e => e.IdPrescripcion).HasColumnName("ID_Prescripcion");
            entity.Property(e => e.Dosis)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Duracion)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Frecuencia)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IdDiagnostico).HasColumnName("ID_Diagnostico");
            entity.Property(e => e.IdMedicamento).HasColumnName("ID_Medicamento");
            entity.Property(e => e.Instrucciones)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.IdDiagnosticoNavigation).WithMany(p => p.Prescripcions)
                .HasForeignKey(d => d.IdDiagnostico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescripcion_Diagnostico");

            entity.HasOne(d => d.IdMedicamentoNavigation).WithMany(p => p.Prescripcions)
                .HasForeignKey(d => d.IdMedicamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Prescripcion_Medicamento");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__202AD22018F83FD4");

            entity.ToTable("Rol", "Administracion");

            entity.Property(e => e.IdRol).HasColumnName("ID_Rol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.NombreRol)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("Nombre_Rol");
        });

        modelBuilder.Entity<UsuarioSistema>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario___DE4431C51441D249");

            entity.ToTable("Usuario_Sistema", "Administracion");

            entity.HasIndex(e => e.Username, "UQ__Usuario___536C85E48832D835").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("ID_Usuario");
            entity.Property(e => e.Estado).HasDefaultValue(true);
            entity.Property(e => e.IdRol).HasColumnName("ID_Rol");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .IsUnicode(false)
                .HasColumnName("Password_hash");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UsuarioSistemas)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuario_Rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
