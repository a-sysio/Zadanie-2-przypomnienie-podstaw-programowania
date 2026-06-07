from datetime import date

class Car:
    DAILY_RATES = {
        "economy": 150,
        "standard": 250,
        "premium": 450,
    }

    def __init__(self, plate: str, brand: str, model: str, car_type: str):
        self.plate = plate
        self.brand = brand
        self.model = model
        self.car_type = car_type.lower()
        self.available = True

    @property
    def daily_rate(self) -> float:
        return self.DAILY_RATES.get(self.car_type, 200)

    def info(self):
        return (f"[{self.plate}] {self.brand} {self.model} "
                f"({self.car_type}, {self.daily_rate} zł/dzień)")

    def __str__(self):
        status = "dostępny" if self.available else "wynajęty"
        return f"{self.info()} – {status}"


class Customer:
    def __init__(self, customer_id: int, name: str, email: str):
        self.customer_id = customer_id
        self.name = name
        self.email = email
        self.rentals: list["Rental"] = []

    def add_rental(self, rental: "Rental"):
        self.rentals.append(rental)

    def total_spent(self) -> float:
        return sum(r.total_cost() for r in self.rentals)

    def __str__(self):
        return f"Klient #{self.customer_id}: {self.name} ({self.email})"


class Rental:
    _next_id = 1

    def __init__(self, customer: Customer, car: Car,
                 start: date, end: date):
        if end <= start:
            raise ValueError("Data zwrotu musi być późniejsza niż data odbioru.")
        if not car.available:
            raise ValueError(f"Samochód {car.plate} jest już wynajęty.")

        self.rental_id = Rental._next_id
        Rental._next_id += 1

        self.customer = customer
        self.car = car
        self.start = start
        self.end = end

        car.available = False
        customer.add_rental(self)

    @property
    def days(self) -> int:
        return (self.end - self.start).days

    def total_cost(self) -> float:
        return self.days * self.car.daily_rate

    def return_car(self):
        self.car.available = True
        print(f"Auto {self.car.plate} zwrócone. "
              f"Łączny koszt: {self.total_cost():.2f} zł")

    def __str__(self):
        return (f"Rezerwacja #{self.rental_id}: {self.customer.name} | "
                f"{self.car.brand} {self.car.model} | "
                f"{self.start} → {self.end} | "
                f"{self.days} dni | {self.total_cost():.2f} zł")


class RentalAgency:
    def __init__(self, name: str):
        self.name = name
        self.fleet: list[Car]     = []
        self.customers: list[Customer] = []
        self.rentals: list[Rental]  = []

    def add_car(self, car: Car):
        self.fleet.append(car)

    def add_customer(self, customer: Customer):
        self.customers.append(customer)

    def create_customer(self, name: str, email: str) -> Customer:
        customer = Customer(len(self.customers) + 1, name, email)
        self.add_customer(customer)
        return customer

    def available_cars(self) -> list[Car]:
        return [c for c in self.fleet if c.available]

    def unavailable_cars(self) -> list[Car]:
        return [c for c in self.fleet if not c.available]

    def rental_for_car(self, car: Car) -> Rental | None:
        for rental in self.rentals:
            if rental.car == car:
                return rental
        return None

    def rent(self, customer: Customer, car: Car,
             start: date, end: date) -> Rental:
        rental = Rental(customer, car, start, end)
        self.rentals.append(rental)
        return rental

def menu():
    agency = RentalAgency("AutoRent")

    agency.add_car(Car("WA1234X", "Toyota", "Yaris", "economy"))
    agency.add_car(Car("LO5678Y", "Volkswagen", "Passat", "standard"))
    agency.add_car(Car("KR9012Z", "BMW", "5 Series", "premium"))
    agency.add_car(Car("GD3456A", "Ford", "Fiesta", "economy"))
    agency.add_car(Car("PO7890B", "Skoda", "Octavia", "standard"))
    agency.add_car(Car("WR2345C", "Audi", "A4", "premium"))
    agency.add_car(Car("LU6789D", "Hyundai", "i30", "economy"))
    agency.add_car(Car("SZ1122E", "Kia", "Ceed", "standard"))
    agency.add_car(Car("BI3344F", "Mercedes", "E-Class", "premium"))
    agency.add_car(Car("RZ5566G", "Renault", "Clio", "economy"))

    def wait_for_menu():
        input("\nNaciśnij Enter, aby wrócić do menu...")

    while True:
        print("\n|------------------------------|")
        print("|   WYPOŻYCZALNIA SAMOCHODÓW   |")
        print("|------------------------------|")
        print("| 1. Pokaż dostępne auta       |")
        print("| 2. Wypożycz samochód         |")
        print("| 3. Zwróć samochód            |")
        print("| 0. Wyjście                   |")
        print("|------------------------------|")

        match input("Wybór: ").strip():

            case "1":
                dostepne = agency.available_cars()
                niedostepne = agency.unavailable_cars()
                print(f"\nDostępne auta ({len(dostepne)}):")
                for car in dostepne:
                    print(f"  {car.info()}")
                print(f"\nNiedostępne auta ({len(niedostepne)}):")
                if not niedostepne:
                    print("  Brak niedostępnych aut.")
                for car in niedostepne:
                    rental = agency.rental_for_car(car)
                    if rental:
                        print(f"  {car.info()} - do {rental.end}")
                    else:
                        print(f"  {car.info()}")

            case "2":
                dostepne = agency.available_cars()
                if not dostepne:
                    print("Brak dostępnych aut.")
                else:
                    print("\nDostępne auta:")
                    for i, car in enumerate(dostepne, 1):
                        print(f"  {i}. {car}")
                    try:
                        nr_auta   = int(input("Wybierz auto (nr): ")) - 1
                        name = input("Imię i nazwisko: ").strip()
                        email = input("Email: ").strip()
                        start_str = input("Data odbioru (RRRR-MM-DD): ")
                        end_str   = input("Data zwrotu  (RRRR-MM-DD): ")
                        if not name or not email:
                            raise ValueError("Imię, nazwisko i email są wymagane.")
                        start = date.fromisoformat(start_str)
                        end   = date.fromisoformat(end_str)
                        customer = agency.create_customer(name, email)
                        r = agency.rent(customer, dostepne[nr_auta], start, end)
                        print(f"\n✔ {r}")
                    except (ValueError, IndexError) as e:
                        print(f"Błąd: {e}")

            case "3":
                aktywne = agency.rentals
                if not aktywne:
                    print("Brak aktywnych wypożyczeń.")
                else:
                    for i, r in enumerate(aktywne, 1):
                        print(f"  {i}. {r}")
                    try:
                        nr = int(input("Wybierz rezerwację do zwrotu (nr): ")) - 1
                        aktywne[nr].return_car()
                        agency.rentals.pop(nr)
                    except (ValueError, IndexError):
                        print("Nieprawidłowy wybór.")

            case "0":
                print("Do widzenia!"); break

            case _:
                print("Nieprawidłowa opcja.")

        wait_for_menu()


if __name__ == "__main__":
    menu()
