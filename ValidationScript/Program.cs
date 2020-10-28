using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ValidationScript
{
    class Program
    {
        private class Contact
        {
            public string fullName { get; set; }
            public string emailAddress { get; set; }
            public string phoneNumber { get; set; }
            public string cityName { get; set; }
            [JsonIgnore]
            public bool Valid { get; set; }
            [JsonIgnore]
            public string Message { get; set; }
        }

        static void Main(string[] args)
        {
            var contacts = ReadContacts(args[0]);
            Validate(contacts);
            var cities = AggregateCities(contacts);
            ReportContacts(contacts);
            ReportCities(cities);
        }

        private static IEnumerable<Contact> ReadContacts(string filename)
        {
            var json = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<Contact[]>(json);
        }

        private static void Validate(IEnumerable<Contact> contacts)
        {
            foreach (var contact in contacts)
            {
                Validate(contact);
            }
        }

        private static void Validate(Contact contact)
        {
            var emailRegex = new Regex(@"^[^@]+@[^@]+$");
            var phoneRegex = new Regex(@"^[\d\-\ ]+$");
            var emailValid = emailRegex.IsMatch(contact.emailAddress);
            var phoneValid = phoneRegex.IsMatch(contact.phoneNumber);
            if (emailValid && phoneValid)
            {
                contact.Valid = true;
                contact.Message = "Valid";
            }
            else if (emailValid)
            {
                contact.Valid = false;
                contact.Message = "Phone is invalid.";
            }
            else if (phoneValid)
            {
                contact.Valid = false;
                contact.Message = "Email is invalid.";
            }
            else
            {
                contact.Valid = false;
                contact.Message = "Email and Phone are invalid.";
            }
        }

        private static IDictionary<string, int> AggregateCities(IEnumerable<Contact> contacts)
        {
            var cities = new Dictionary<string, int>();
            foreach (var city in contacts.Select(c => c.cityName).Distinct())
            {
                cities.Add(city, 0);
            }
            foreach (var contact in contacts.Where(c => !c.Valid))
            {
                cities[contact.cityName]++;
            }
            return cities;
        }

        private static void ReportContacts(IEnumerable<Contact> contacts)
        {
            foreach (var contact in contacts.OrderBy(c => c.fullName))
            {
                Console.WriteLine($"{contact.fullName}\t{contact.Message}");
            }
        }

        private static void ReportCities(IDictionary<string, int> cities)
        {
            foreach (var city in cities.OrderByDescending(c => c.Value))
            {
                Console.WriteLine($"{city.Key}\t{city.Value}");
            }
        }
    }
}
