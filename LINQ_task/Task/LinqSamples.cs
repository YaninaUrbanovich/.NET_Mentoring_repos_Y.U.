// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
    [Title("LINQ Module")]
    [Prefix("Linq")]
    public class LinqSamples : SampleHarness
    {

        private DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Where - Task 1")]
        [Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
        public void Linq1()
        {
            int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

            var lowNums =
                from num in numbers
                where num < 5
                select num;

            Console.WriteLine("Numbers < 5:");
            foreach (var x in lowNums)
            {
                Console.WriteLine(x);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 2")]
        [Description("This sample return return all presented in market products")]

        public void Linq2()
        {
            var products =
                from p in dataSource.Products
                where p.UnitsInStock > 0
                select p;

            foreach (var p in products)
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Point 1")]
        [Description("This sample shows the list of all customers whose total orders' sum is more than X")]

        public void Linq3()
        {
            int x = 300;
            var customers =
                from c in dataSource.Customers
                where c.Orders.Sum(o => o.Total) > x
                select new { c.CustomerID, Total = c.Orders.Sum(o => o.Total) };
            Console.WriteLine($"Customers whose total orders' sum is more than {x}");
            foreach (var customer in customers)
            {
                Console.WriteLine($"Customer: {customer.CustomerID} Total: {customer.Total}");
            }
        }

        [Category("Grouping Operators")]
        [Title("Select/GroupJoin - Point 2")]
        [Description("This sample shows the list of suppliers that are in the same city and country as their clients")]

        public void Linq4()
        {
            var customersAndSuppliers =
                from c in dataSource.Customers
                from s in dataSource.Suppliers
                where c.City == s.City && c.Country == s.Country
                select new { c.CustomerID, Supplier = s.SupplierName };

            foreach (var customer in customersAndSuppliers)
            {
                ObjectDumper.Write($"Customer: {customer.CustomerID} Suppliers: {customer.Supplier}");
            }

            var customersSuppliers = dataSource.Customers.GroupJoin(dataSource.Suppliers,
                c => new { c.City, c.Country },
                s => new { s.City, s.Country },
                (c, s) => new { Customer = c.CompanyName, Suppliers = s.AsEnumerable() }).Where(x => x.Suppliers.Count() > 0);

            foreach (var c in customersSuppliers)
            {
                Console.WriteLine($"Customer: {c.Customer}");
                c.Suppliers.ToList().ForEach(s => Console.WriteLine($"Supplier: {s.SupplierName}"));
            }


        }
        [Category("Restriction Operators")]
        [Title("Where - Point 3")]
        [Description("This sample shows the list of all customers whose total orders' sum is more than X")]

        public void Linq5()
        {
            int x = 1000;
            var customers =
                from c in dataSource.Customers
                where c.Orders.Any(o => o.Total > x)
                select (c);
            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }


        }
        [Category("Ordering Operators")]
        [Title("OrderBy - Point 4")]
        [Description("This sample shows the list of the customers which indicate the month and the year of the first order")]

        public void Linq6()
        {
            var customers =
                from c in dataSource.Customers
                where c.Orders.Any()
                select new { c.CustomerID, StartDate = c.Orders.OrderBy(x => x.OrderDate).First() };
            foreach (var c in customers)
            {
                Console.WriteLine($"Customer: {c.CustomerID} Month: {c.StartDate.OrderDate.Month} Year: {c.StartDate.OrderDate.Year}");
            }

        }

        [Category("Ordering Operators")]
        [Title("OrderByDescending - Point 5")]
        [Description("This sample shows the list of the customers which indicate the month and the year of the first order" +
            "But is sorted by year, month, total orders' sum and client name")]

        public void Linq7()
        {
            var customers =
                from c in dataSource.Customers.Where(c => c.Orders.Any())
                let startDate = c.Orders.OrderBy(x => x.OrderDate).FirstOrDefault().OrderDate
                let sum = c.Orders.Sum(o => o.Total)
                orderby startDate, sum, c.CompanyName descending
                select new { c.CompanyName, StartDate = startDate, Total = sum };
            foreach (var c in customers)
            {
                Console.WriteLine($"Customer: {c.CompanyName} Month: {c.StartDate.Month} Year: {c.StartDate.Year} Sum: {c.Total}");
            }

        }
        [Category("Grouping Operators")]
        [Title("OrderBy - Nested - Point 7")]
        [Description("This sample shows the products grouped by categories, units in stock and ordered by price.")]

        public void Linq8()
        {
            var products =
                from p in dataSource.Products
                group p by p.Category into c
                select
                new
                {
                    Category = c.Key,
                    ProductsStock =
                  from s in c
                  group s by s.UnitsInStock > 0 into x
                  select
                  new
                  {
                      InStock = x.Key,
                      Products =
                      from y in x
                      orderby y.UnitPrice
                      select y
                  }

                };
            foreach (var p in products)
            {
                Console.WriteLine($"Category: {p.Category}");
                foreach (var productsStock in p.ProductsStock)
                {
                    Console.WriteLine($"In Stock: {productsStock.InStock}");
                    foreach (var product in productsStock.Products)
                    {
                        Console.WriteLine($"Product: {product.ProductName} Price: {product.UnitPrice}");
                    }
                }
            }

        }
        [Category("Grouping Operators")]
        [Title("GroupBy - Point 8")]
        [Description("This sample shows the products grouped by price: cheap, average and expensive")]

        public void Linq9()
        {
            int c = 80;
            int e = 150;

            var products =
                from p in dataSource.Products
                group p by p.UnitPrice < c ? "Cheap" : (p.UnitPrice > e ? "Expensive" : "Average") into x
                select new
                {
                    Price = x.Key,
                    Products = from y in x
                               select y
                };

            foreach (var p in products)
            {
                Console.WriteLine($"{p.Price}");
                foreach (var product in p.Products)
                {
                    Console.WriteLine($"Product: {product.ProductName} Price: {product.UnitPrice}");
                }
            }

        }
        [Category("Grouping Operators")]
        [Title("GroupBy - Point 9")]
        [Description("This sample shows the average profitability of the each city (the average client's order sum from the definite city) and the average intensity (the average amount of the orders from each city)")]

        public void Linq10()
        {

            var customers =
                from c in dataSource.Customers
                group c by c.City into y
                select new
                {
                    City = y.Key,
                    AverageSum = y.Average(s => s.Orders.Sum(o => o.Total)),
                    Intensity = y.Average(i => i.Orders.Length)
                };

            foreach (var c in customers)
            {
                Console.WriteLine($"City: {c.City}");
                Console.WriteLine($"Average Sum: { c.AverageSum}");
                Console.WriteLine($"Intensity: { c.Intensity}");
            }


        }
        [Category("Grouping Operators")]
        [Title("GroupBy - Nested - Point 10")]
        [Description("This sample shows the average profitability of the each city (the average client's order sum from the definite city) and the average intensity (the average amount of the orders from each city)")]

        public void Linq11()
        {

            var customers =
                from c in dataSource.Customers
                select new
                {
                    c.CustomerID,
                    MonthStatistics =
                    from o in c.Orders
                    group o by o.OrderDate.Month into mg
                    select new { Month = mg.Key},
                    YearStatistics =
                    from o in c.Orders
                    group o by o.OrderDate.Year into yg
                    select new { Year = yg.Key},
                    YearAndMonthStatistics=
                    from o in c.Orders
                    group o by $"{o.OrderDate.Month} / {o.OrderDate.Year}" into ymg
                    select new { MonthAndYear = ymg.Key}
                };

            foreach (var c in customers)
            {
                Console.WriteLine($"Customer: {c.CustomerID}");
                foreach (var month in c.MonthStatistics)
                {
                    Console.WriteLine($"Month: {month.Month}");
                }
                foreach (var year in c.YearStatistics)
                {
                    Console.WriteLine($"Year: {year.Year}");
                }
                foreach (var yearAndMonth in c.YearAndMonthStatistics)
                {
                    Console.WriteLine($"Year and Month: {yearAndMonth.MonthAndYear}");
                }
            }


        }

    }
}
