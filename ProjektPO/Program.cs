using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static KsiegarniaApp.Program;

namespace KsiegarniaApp
{
    public class Program
    {
        //dostępne role w programie
        public enum Role
        {
            Admin,
            Worker,
            Client
        }
        //dostępne uprawnienia w programie
        public enum Permission
        {
            ManageUsers,
            AddBook,
            RemoveBook,
            EditBook,
            ShowBooks,
            BuyBook,
        }
        public interface IInfo
        {
            string DisplayInfo();
        }
        public class Book : IInfo
        {
            private double _cena;
            private int _ilosc;
            public string Tytul { get; set; }
            public string Autor { get; set; }
            public double Cena
            {
                get { return _cena; }
                set
                {
                    if (value <= 0)
                        throw new ArgumentException("Cena nie może być mniejsza lub równa 0");
                    _cena = value;
                }
            }
            public int Ilosc
            {
                get { return _ilosc; }
                set
                {
                    if (value < 0)
                        throw new ArgumentException("Ilość książek nie może być mniejsza od 0");
                    _ilosc = value;
                }
            }

            public Book() { }
            public Book(string tytul, string autor, double cena, int ilosc)
            {
                Tytul = tytul;
                Autor = autor;
                Cena = cena;
                Ilosc = ilosc;
            }
            public string DisplayInfo()
            {
                return ($"\"{Tytul}\" {Autor}, kosztuje: {Cena}");
            }
        }
        // Delegat i Event
        public delegate void PowiadomienieEventHandler(object sender, string message);
        public class Osoba : IInfo
        {
            public string Username { get; set; }
            public List<Role> Roles;
            public List<Book> BoughtBooks { get; set; } = new List<Book>();


            public event PowiadomienieEventHandler Powiadomienie;
            protected virtual void OnPowiadomienie(string message)
            {
                Powiadomienie?.Invoke(this, message);
            }

            public Osoba(string imie)
            {
                Username = imie;
                Roles = new List<Role>();
            }
            public string DisplayInfo()
            {
                string output = $"Nazwa użytkownika: {Username}, posiadane role: ";
                foreach (Role role in Roles)
                {
                    output += $"{role}, ";
                }
                return output;
            }
            public void KupioneKsiazki()
            {
                Console.WriteLine("Historia kupowania:");
                foreach (var k in BoughtBooks)
                {
                    Console.WriteLine($"\t - {k.DisplayInfo()}");
                }
            }

            public void AddBook()
            {
                Console.WriteLine($"\nWybierz kolejną opcję:");
                Console.WriteLine($"\t1. Dokupić instniejącą książkę");
                Console.WriteLine($"\t2. Kupić nową książkę");
                int choice = GetIntInput();
                switch (choice)
                {
                    case 1:
                        Console.WriteLine(Ksiegarnia.Instance.DisplayInfo());
                        Console.WriteLine("Podaj tytuł książki, którą chcecz dokupić: ");
                        string title = Console.ReadLine();
                        foreach (var book in Ksiegarnia.Instance.Books)
                        {
                            if(book.Tytul.ToLower() == title.ToLower())
                            {
                                Console.WriteLine($"Podaj ile książek \"{book.Tytul}\" {book.Autor} chcesz dokupić: ");
                                int howMany = GetIntInput();
                                book.Ilosc += howMany;
                                OnPowiadomienie($"Dodano {howMany} książkek \"{book.Tytul}\"");
                                return;
                            }
                        }
                        Console.WriteLine($"Nie ma książki o tytule \"{title}\"");
                        break;
                    case 2:
                        Book newbook = new Book();
                        try
                        {
                            Console.WriteLine("\nPodaj tytuł książki, którą chcecz kupić: ");
                            newbook.Tytul = Console.ReadLine();
                            Console.WriteLine("\nPodaj autora książki, którą chcecz kupić: ");
                            newbook.Autor = Console.ReadLine();
                            Console.WriteLine("\nPodaj cenę książki, którą chcecz kupić: ");
                            double.TryParse(Console.ReadLine(), out double cena);
                            newbook.Cena = cena;
                            Console.WriteLine($"\nPodaj ile książek o tytule {newbook.Tytul}, chcecz kupić: ");
                            newbook.Ilosc = int.Parse(Console.ReadLine());
                            OnPowiadomienie($"Dodano {newbook.Ilosc} książkek \"{newbook.Tytul}\" {newbook.Autor} w cenie {newbook.Cena}");
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine("Błąd: "+e.Message);
                        }

                        break;
                    default:
                        Console.WriteLine("Nieznana opcja");
                        return;
                }
            }
            public void RemoveBook()
            {
                Console.WriteLine(Ksiegarnia.Instance.DisplayInfo());
                Console.WriteLine("Podaj tytuł książki, którą chcecz usunąć: ");
                string title = Console.ReadLine();
                foreach (var book in Ksiegarnia.Instance.Books)
                {
                    if (book.Tytul.ToLower() == title.ToLower())
                    {
                        Console.WriteLine($"Podaj ile książek \"{book.Tytul}\" {book.Autor} chcesz sprzedać: ");
                        int howMany = GetIntInput();
                        if (howMany > book.Ilosc)
                        {
                            Console.WriteLine("Nie można usunąć więcej książek niż ich jest");
                            return;
                        }
                        book.Ilosc -= howMany;
                        OnPowiadomienie($"Usunięto {howMany} książkek \"{book.Tytul}\"");
                        return;
                    }
                }
                Console.WriteLine($"Nie ma książki o tytule \"{title}\"");
            }
            public void EditBook()
            {
                Console.WriteLine(Ksiegarnia.Instance.DisplayInfo());
                Console.WriteLine("Podaj tytuł książki, w której chcecz coś zmienić: ");
                string title = Console.ReadLine();
                foreach (var book in Ksiegarnia.Instance.Books)
                {
                    try
                    {
                        if (book.Tytul.ToLower() == title.ToLower())
                        {
                            Console.WriteLine($"Podaj co chcesz zmienić w tej książce: ");
                            Console.WriteLine($"\t1. Tytuł ");
                            Console.WriteLine($"\t2. Autora ");
                            Console.WriteLine($"\t3. Cenę ");
                            int choice = GetIntInput();
                            switch (choice)
                            {
                                case 1:
                                    Console.WriteLine($"\nObecny tytuł: {book.Tytul}\nPodaj na jaki tytuł chcesz go zmienić:");
                                    string title2 = Console.ReadLine();
                                    book.Tytul = title2;
                                    OnPowiadomienie($"Zmieniono tytuł książki \"{title}\" na \"{title2}\"");
                                    return;
                                case 2:
                                    Console.WriteLine($"\nObecny autor: {book.Autor}\nPodaj na jakiego autora chcesz go zmienić:");
                                    string autor = Console.ReadLine();
                                    string oldAutor = book.Autor;
                                    book.Autor = autor;
                                    OnPowiadomienie($"Zmieniono autora książki \"{title}\" z {oldAutor} na {autor}");
                                    return;
                                case 3:
                                    Console.WriteLine($"\nObecna cena: {book.Cena:C}\nPodaj na jaką cenę chcesz ją zmienić:");
                                    double cena;
                                    double.TryParse(Console.ReadLine(), out cena);
                                    double oldCena = book.Cena;
                                    book.Cena = cena;
                                    OnPowiadomienie($"Zmieniono cenę książki \"{title}\" z {oldCena:C} na {cena:C}");
                                    return;
                                default:
                                    Console.WriteLine("Nieznana opcja");
                                    return;
                            }
                        }
                    }
                    catch (ArgumentException e)
                    {
                        Console.WriteLine("Błąd: " + e.Message);
                        return;
                    }
                    
                }
                Console.WriteLine($"Nie ma książki o tytule \"{title}\"");
            }
            public void BuyBook()
            {
                //Do zrobienia
            }
            public void ManageUsers()
            {
                //Do zrobienia
            }
        }
    
        public class Ksiegarnia : IInfo
        {
            private static Ksiegarnia _instance;
            public static Ksiegarnia Instance
            {
                get
                {
                    if (_instance == null)
                    {
                        _instance = new Ksiegarnia();
                    }
                    return _instance;
                }
            }

            public List<Book> Books { get; set; } = new List<Book>();

            public string DisplayInfo()
            {
                string output = $"Księgarnia, tytuły posiadanych książek:";
                foreach (Book book in Books)
                {
                    output += $"\n - \"{book.Tytul}\", cena: {book.Cena:C}, liczba książek w księgarni: {book.Ilosc}";
                }
                return output;
            }

            private Ksiegarnia() //Dodać tutaj śćiąganie książek z bazy danych
            {
                // Inicjalizacja przykładowych książek w pamięci
                Books.Add(new Book("Wiedźmin", "Andrzej Sapkowski", 29.99d, 100));
                Books.Add(new Book("Lalka", "Bolesław Prus", 39.98d, 28));
                Books.Add(new Book("Pan Tadeusz", "Adam Mickiewicz", 12.50d, 13));
            }
        }
        //RBAC
        public class RBAC
        {
            private readonly Dictionary<Role, List<Permission>> _rolePermissions; 
            public RBAC()
            {
                _rolePermissions = new Dictionary<Role, List<Permission>>
                    {
                        { Role.Admin, new List<Permission> {Permission.ManageUsers, Permission.AddBook, Permission.RemoveBook, Permission.EditBook, Permission.ShowBooks} },
                        { Role.Worker, new List<Permission> {Permission.AddBook, Permission.RemoveBook, Permission.EditBook, Permission.ShowBooks} },
                        { Role.Client, new List<Permission> {Permission.ShowBooks, Permission.BuyBook} },
                    };
            }

            public bool HasPermission(Osoba user, Permission permission)
            {
                foreach (var role in user.Roles)
                {
                    if (_rolePermissions.ContainsKey(role) && _rolePermissions[role].Contains(permission))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    
        //funkcja zmieniająca kolor tekstu, często ją używam i ona przyśpieszy mi pisanie kodu
        public static void ChangeColor(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static int GetIntInput()
        {
            int input = -1;
            while (input == -1)
            {
                try
                {
                    input = int.Parse(Console.ReadLine());
                    if (input < 0)
                    {
                        input = -1;
                        throw new ArgumentException("Wybór musi być liczbą naturalną");
                    }
                }
                catch (ArgumentNullException)
                {
                    ChangeColor($"Błąd: Wybór nie może być pusty", ConsoleColor.Red);
                }
                catch (ArgumentException e)
                {
                    ChangeColor($"Błąd: {e.Message}", ConsoleColor.Red);
                }
                catch (FormatException)
                {
                    ChangeColor($"Błąd: Wybór musi być liczbą całkowitą", ConsoleColor.Red);
                }
                catch (Exception e)
                {
                    ChangeColor($"Błąd: {e.Message}", ConsoleColor.Red);
                }
            }
            return input;
        }
        public static Osoba Login() //dodać hasło
        {
            Console.Write("Podaj imię: ");
            string imie = Console.ReadLine();
            if (Uzytkownicy.ContainsKey(imie))
                return Uzytkownicy[imie];
            else
            {
                return Uzytkownicy["admin1"];   //BŁĄD: zawsze zaloguje ciebie na jakieś konto, Trzeba tutaj coś dodać
            }
        }
        public static void Rejestracja() //dodać hasło
        {
            Console.Write("\nPodaj nazwę użytkownika: ");
            string imie = Console.ReadLine();

            if (string.IsNullOrEmpty(imie))
            {
                Console.WriteLine("Imię nie może być puste.");
                return;
            }
            Osoba osoba = new Osoba(imie);

            Console.WriteLine("\nWybierz swoją główną rolę:");
            Console.WriteLine("\n\t1. Admin");
            Console.WriteLine("\n\t2. Pracownik");
            Console.WriteLine("\n\t3. Klient");

            int role = GetIntInput();
            switch (role)
            {
                case 1:
                    osoba.Roles.Add(Role.Admin);
                    break;
                case 2:
                    osoba.Roles.Add(Role.Worker);
                    break;
                case 3:
                    osoba.Roles.Add(Role.Client);
                    break;
            }

            Uzytkownicy[imie] = osoba;
        }

        public static void OnPowiadomienieReceived(object sender, string message)
        {
            Console.WriteLine("Powiadomienie: " + message);
        }

        static Dictionary<string, Osoba> Uzytkownicy = new Dictionary<string, Osoba>(); //Trzeba by to zmienić, można stworzyć że czytamy z pliku i tutaj zapisujemy
        static void Main()
        {
            Osoba admin1 = new Osoba("admin1");
            admin1.Roles.Add(Role.Admin);
            admin1.Roles.Add(Role.Worker);
            admin1.Roles.Add(Role.Client);
            Uzytkownicy.Add(admin1.Username, admin1);
            admin1.Powiadomienie += OnPowiadomienieReceived;    // Subskrypcja eventu

            ChangeColor(admin1.DisplayInfo(), ConsoleColor.Magenta);
            ChangeColor(Ksiegarnia.Instance.DisplayInfo(), ConsoleColor.Magenta);
            Console.ReadKey();

            while (true)
            {
                Console.Clear();
                ChangeColor("==Witamy w systemie Logowania Księgarni==", ConsoleColor.DarkGray);
                Console.WriteLine("Proszę wybrać jedną z opcji:");
                Console.WriteLine("\t0. Wyjdź z programu");
                Console.WriteLine("\t1. Zaloguj się");
                Console.WriteLine("\t2. Zarejestruj się\n");
                int input = GetIntInput();
                switch (input)
                {
                    case 0:
                        Console.WriteLine("Dziękujemy za skorzysanie z naszego programu");
                        Task.Delay(1000).Wait();    //Sprawia, że consola czeka 1000 milisekund
                        return;
                    case 1:
                        Osoba user = Login();
                        ChangeColor("Logowanie powiodło się", ConsoleColor.Green);
                        ChangeColor("--Kliknij, aby kontynuować działanie programu--", ConsoleColor.DarkGray);
                        Console.ReadKey();
                        Choice(user);
                        break;
                    case 2:
                        Rejestracja();
                        ChangeColor("Rejestracja powiodła się", ConsoleColor.Green);
                        break;
                    default:
                        ChangeColor("Rejestracja powiodła się", ConsoleColor.Red);
                        break;
                }
                ChangeColor("--Kliknij, aby kontynuować działanie programu--", ConsoleColor.DarkGray);
                Console.ReadKey();
            }

        }
        public static void Choice(Osoba osoba)
        {
            RBAC rbac = new RBAC();
            bool running = true;
            while (running)
            {
                Console.Clear();
                ChangeColor("==Witamy w systemie Menu Księgarni==", ConsoleColor.DarkGray);
                Console.WriteLine("Twoje opcje:");
                Console.WriteLine("\t1. Pokaż książki w księgarnii");
                Console.WriteLine("\t2. Kup książkę");
                Console.WriteLine("\t3. Historia kupowania");
                Console.WriteLine("\t4. Dodaj książkę");
                Console.WriteLine("\t5. Usuń książkę");
                Console.WriteLine("\t6. Edytuj książkę");
                Console.WriteLine("\t7. Manage users");
                Console.WriteLine("\t0. Wyloguj się\n");
                int choice = GetIntInput();
                switch (choice)
                {
                    case 1:
                        if (rbac.HasPermission(osoba, Permission.ShowBooks))
                            Console.WriteLine(Ksiegarnia.Instance.DisplayInfo());
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 2:
                        if (rbac.HasPermission(osoba, Permission.BuyBook))
                            osoba.BuyBook();
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 3:
                        if (rbac.HasPermission(osoba, Permission.BuyBook))
                            osoba.KupioneKsiazki();
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 4:
                        if (rbac.HasPermission(osoba, Permission.AddBook))
                            osoba.AddBook();
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 5:
                        if (rbac.HasPermission(osoba, Permission.RemoveBook))
                            osoba.RemoveBook();
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 6:
                        if (rbac.HasPermission(osoba, Permission.EditBook))
                            osoba.EditBook();
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 7:
                        if (rbac.HasPermission(osoba, Permission.ManageUsers))
                            osoba.ManageUsers();
                        else
                            Console.WriteLine("Nie masz pozwolenia na korzystanie z tej funkcji.");
                        break;
                    case 0:
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Nieznana opcja");
                        break;
                }
                ChangeColor("--Kliknij, aby kontynuować działanie programu--", ConsoleColor.DarkGray);
                Console.ReadKey();
            }
        }
    }
}
