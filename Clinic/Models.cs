using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace WpfApp20.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Логин { get; set; }
        public string Пароль { get; set; }
        public string Роль { get; set; }
    }

    public class Лечение
    {
        [Key]
        public int Id { get; set; }
        public string Название { get; set; }
        public string Описание { get; set; }
        public decimal Стоимость { get; set; }
        public int Продолжительность { get; set; }
    }

    public class Пациенты
    {
        [Key]
        public int Id { get; set; }
        public string ФИО { get; set; }
        public DateTime? Дата_рождения { get; set; }
        public string Пол { get; set; }
        public string Номер_телефона { get; set; }
        public string Е_мейл { get; set; }
        public string Паспорт { get; set; }
    }

    public class Персонал
    {
        [Key]
        public int Id { get; set; }
        public string ФИО { get; set; }
        public string Должность { get; set; }
        public string Телефон { get; set; }
    }

    public class Отчеты
    {
        [Key]
        public int Id { get; set; }
        public int id_lec { get; set; }
        public int id_pac { get; set; }
        public int id_perc { get; set; }
        public DateTime Дата_начала { get; set; }
        public DateTime Дата_окончания { get; set; }
        public string Кабинет { get; set; }
        public string Описание { get; set; }

        [ForeignKey("id_lec")]
        public virtual Лечение Лечение { get; set; }
        [ForeignKey("id_pac")]
        public virtual Пациенты Пациенты { get; set; }
        [ForeignKey("id_perc")]
        public virtual Персонал Персонал { get; set; }
    }

    public class Записи
    {
        [Key]
        public int Id { get; set; }
        public int id_lec { get; set; }
        public int id_pac { get; set; }
        public int id_perc { get; set; }
        public DateTime Дата { get; set; }
        public string Время { get; set; }
        public string Кабинет { get; set; }

        [ForeignKey("id_lec")]
        public virtual Лечение Лечение { get; set; }
        [ForeignKey("id_pac")]
        public virtual Пациенты Пациенты { get; set; }
        [ForeignKey("id_perc")]
        public virtual Персонал Персонал { get; set; }
    }
    public class DiseaseStat : INotifyPropertyChanged
    {
        private string _disease;
        private int _count;
        private double _percentage;

        public string Disease
        {
            get => _disease;
            set { _disease = value; OnPropertyChanged(); }
        }
        public int Count
        {
            get => _count;
            set { _count = value; OnPropertyChanged(); }
        }
        public double Percentage
        {
            get => _percentage;
            set { _percentage = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class AttendanceStat : INotifyPropertyChanged
    {
        private string _month;
        private int _count;

        public string Month
        {
            get => _month;
            set { _month = value; OnPropertyChanged(); }
        }
        public int Count
        {
            get => _count;
            set { _count = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DoctorStat : INotifyPropertyChanged
    {
        private string _doctorName;
        private int _appointmentCount;
        private int _patientCount;
        private double _averageAppointmentsPerPatient;

        public string DoctorName
        {
            get => _doctorName;
            set { _doctorName = value; OnPropertyChanged(); }
        }
        public int AppointmentCount
        {
            get => _appointmentCount;
            set { _appointmentCount = value; OnPropertyChanged(); }
        }
        public int PatientCount
        {
            get => _patientCount;
            set { _patientCount = value; OnPropertyChanged(); }
        }
        public double AverageAppointmentsPerPatient
        {
            get => _averageAppointmentsPerPatient;
            set { _averageAppointmentsPerPatient = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}