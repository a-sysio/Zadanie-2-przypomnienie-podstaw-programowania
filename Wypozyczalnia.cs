using System;
using System.Collections.Generic;

class Car
{
    private static readonly Dictionary<string, double> DailyRates = new()
    {
        { "economy", 150 },
        { "standard", 250 },
        { "premium", 450 },
    };

    public string Plate { get; }
    public string Brand { get; }
    public string Model { get; }
    public string CarType { get; }
    public bool Available { get; set; } = true;

    public double DailyRate =>
        DailyRates.TryGetValue(CarType, out double r) ? r : 200;

    public Car(string plate, string brand, string model, string carType)
    {
        Plate = plate;
        Brand = brand;
        Model = model;
        CarType = carType.ToLower();
    }

    public string Info() =>
        $"[{Plate}] {Brand} {Model} ({CarType}, {DailyRate} zł/dzień)";

    public override string ToString()
    {
        string status = Available ? "dostępny" : "wynajęty";
        return $"{Info()} – {status}";
    }
}


class Customer
{
    public int CustomerId { get; }
    public string Name { get; }
    public string Email { get; }
    public List<Rental> Rentals { get; } = new();

    public Customer(int customerId, string name, string email)
    {
        CustomerId = customerId;
        Name = name;
        Email = email;
    }

    public void AddRental(Rental rental) => Rentals.Add(rental);

    public double TotalSpent()
    {
        double sum = 0;
        foreach (var r in Rentals) sum += r.TotalCost();
        return sum;
    }

    public override string ToString() =>
        $"Klient #{CustomerId}: {Name} ({Email})";
}


class Rental
{
    private static int _nextId = 1;

    public int RentalId { get; }
    public Customer Customer { get; }
    public Car Car { get; }
    public DateTime Start { get; }
    public DateTime End { get; }

    public int Days => (End - Start).Days;
    public double TotalCost() => Days * Car.DailyRate;

    public Rental(Customer customer, Car car, DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("Data zwrotu musi być późniejsza niż data odbioru.");
        if (!car.Available)
            throw new InvalidOperationException($"Samochód {car.Plate} jest już wynajęty.");

        RentalId = _nextId++;
        Customer = customer;
        Car = car;
        Start = start;
        End = end;

        car.Available = false;
        customer.AddRental(this);
    }

    public void ReturnCar()
    {
        Car.Available = true;
        Console.WriteLine($"Auto {Car.Plate} zwrócone. Łączny koszt: {TotalCost():F2} zł");
    }

    public override string ToString() =>
        $"Rezerwacja #{RentalId}: {Customer.Name} | {Car.Brand} {Car.Model} | " +
        $"{Start:yyyy-MM-dd} → {End:yyyy-MM-dd} | {Days} dni | {TotalCost():F2} zł";
}

// ── RentalAgency ─────────────────────────────────────────────
class RentalAgency
{
    public string Name{ get; }
    public List<Car> Fleet{ get; } = new();
    public List<Customer> Customers{ get; } = new();
    public List<Rental> Rentals { get; } = new();

    public RentalAgency(string name) => Name = name;

    public void AddCar(Car car) => Fleet.Add(car);
    public void AddCustomer(Customer c) => Customers.Add(c);

    public Customer CreateCustomer(string name, string email)
    {
        var customer = new Customer(Customers.Count + 1, name, email);
        AddCustomer(customer);
        return customer;
    }

    public List<Car> AvailableCars()
    {
        var list = new List<Car>();
        foreach (var c in Fleet)
            if (c.Available) list.Add(c);
        return list;
    }

    public List<Car> UnavailableCars()
    {
        var list = new List<Car>();
        foreach (var c in Fleet)
            if (!c.Available) list.Add(c);
        return list;
    }

    public Rental? RentalForCar(Car car)
    {
        foreach (var r in Rentals)
            if (r.Car == car) return r;
        return null;
    }

    public Rental Rent(Customer customer, Car car, DateTime start, DateTime end)
    {
        var rental = new Rental(customer, car, start, end);
        Rentals.Add(rental);
        return rental;
    }
}


class Program
{
    static void WaitForMenu()
    {
        Console.WriteLine("\nNaciśnij Enter, aby wrócić do menu...");
        Console.ReadLine();
    }

    static void Main()
    {
        var agency = new RentalAgency("AutoRent");

        agency.AddCar(new Car("WA1234X", "Toyota", "Yaris", "economy"));
        agency.AddCar(new Car("LO5678Y", "Volkswagen", "Passat", "standard"));
        agency.AddCar(new Car("KR9012Z", "BMW", "5 Series", "premium"));
        agency.AddCar(new Car("GD3456A", "Ford", "Fiesta", "economy"));
        agency.AddCar(new Car("PO7890B", "Skoda", "Octavia", "standard"));
        agency.AddCar(new Car("WR2345C", "Audi", "A4", "premium"));
        agency.AddCar(new Car("LU6789D", "Hyundai", "i30", "economy"));
        agency.AddCar(new Car("SZ1122E", "Kia", "Ceed", "standard"));
        agency.AddCar(new Car("BI3344F", "Mercedes", "E-Class", "premium"));
        agency.AddCar(new Car("RZ5566G", "Renault", "Clio", "economy"));

        while (true)
        {
            Console.WriteLine("\n|------------------------------|");
            Console.WriteLine("|   WYPOŻYCZALNIA SAMOCHODÓW   |");
            Console.WriteLine("|------------------------------|");
            Console.WriteLine("| 1. Pokaż dostępne auta       |");
            Console.WriteLine("| 2. Wypożycz samochód         |");
            Console.WriteLine("| 3. Zwróć samochód            |");
            Console.WriteLine("| 0. Wyjście                   |");
            Console.WriteLine("|------------------------------|");
            Console.Write("Wybór: ");

            switch (Console.ReadLine()?.Trim())
            {
                case "1":
                {
                    var dostepne = agency.AvailableCars();
                    var niedostepne = agency.UnavailableCars();

                    Console.WriteLine($"\nDostępne auta ({dostepne.Count}):");
                    foreach (var car in dostepne)
                        Console.WriteLine($"  {car.Info()}");

                    Console.WriteLine($"\nNiedostępne auta ({niedostepne.Count}):");
                    if (niedostepne.Count == 0)
                        Console.WriteLine("  Brak niedostępnych aut.");
                    foreach (var car in niedostepne)
                    {
                        var rental = agency.RentalForCar(car);
                        string suffix = rental != null
                            ? $"- do {rental.End:yyyy-MM-dd}"
                            : "";
                        Console.WriteLine($"  {car.Info()} {suffix}");
                    }
                    break;
                }

                case "2":
                {
                    var dostepne = agency.AvailableCars();
                    if (dostepne.Count == 0)
                    {
                        Console.WriteLine("Brak dostępnych aut.");
                        break;
                    }

                    Console.WriteLine("\nDostępne auta:");
                    for (int i = 0; i < dostepne.Count; i++)
                        Console.WriteLine($"  {i + 1}. {dostepne[i]}");

                    try
                    {
                        Console.Write("Wybierz auto (nr): ");
                        int nrAuta = int.Parse(Console.ReadLine()!) - 1;

                        Console.Write("Imię i nazwisko: ");
                        string name = Console.ReadLine()!.Trim();

                        Console.Write("Email: ");
                        string email = Console.ReadLine()!.Trim();

                        Console.Write("Data odbioru (RRRR-MM-DD): ");
                        DateTime start = DateTime.Parse(Console.ReadLine()!);

                        Console.Write("Data zwrotu  (RRRR-MM-DD): ");
                        DateTime end = DateTime.Parse(Console.ReadLine()!);

                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                            throw new ArgumentException("Imię, nazwisko i email są wymagane.");

                        var customer = agency.CreateCustomer(name, email);
                        var r = agency.Rent(customer, dostepne[nrAuta], start, end);
                        Console.WriteLine($"\n✔ {r}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Błąd: {e.Message}");
                    }
                    break;
                }

                case "3":
                {
                    if (agency.Rentals.Count == 0)
                    {
                        Console.WriteLine("Brak aktywnych wypożyczeń.");
                        break;
                    }

                    for (int i = 0; i < agency.Rentals.Count; i++)
                        Console.WriteLine($"  {i + 1}. {agency.Rentals[i]}");

                    try
                    {
                        Console.Write("Wybierz rezerwację do zwrotu (nr): ");
                        int nr = int.Parse(Console.ReadLine()!) - 1;
                        agency.Rentals[nr].ReturnCar();
                        agency.Rentals.RemoveAt(nr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Błąd: {e.Message}");
                    }
                    break;
                }

                case "0":
                    Console.WriteLine("Do widzenia!");
                    return;

                default:
                    Console.WriteLine("Nieprawidłowa opcja.");
                    break;
            }

            WaitForMenu();
        }
    }
}
