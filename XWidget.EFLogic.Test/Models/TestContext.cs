﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XWidget.EFLogic.Test.Models {
    public class TestContext : DbContext {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Note> Notes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseLazyLoadingProxies();
                optionsBuilder.UseInMemoryDatabase("TestDatabase");
            }
        }

        public static TestContext CreateInstance() {
            var result = new TestContext();

            var category_A = new Category() {
                Name = "Category_A"
            };
            var category_B = new Category() {
                Name = "Category_B"
            };


            category_A.Children.Add(new Category() {
                Name = "Category_A_1"
            });
            category_A.Children.Add(new Category() {
                Name = "Category_A_2"
            });
            category_B.Children.Add(new Category() {
                Name = "Category_B_1"
            });
            category_B.Children.Add(new Category() {
                Name = "Category_B_2"
            });

            result.Categories.Add(category_A);
            result.Categories.Add(category_B);

            result.Notes.Add(new Note() {
                Title = "Note_0",
                Category = category_A,
                Context = "Test Context"
            });
            result.Notes.Add(new Note() {
                Title = "Note_1",
                Category = category_A.Children.First(),
                Context = "Test Context"
            });
            result.Notes.Add(new Note() {
                Title = "Note_2",
                Category = category_A.Children.First(),
                Context = "Test Context"
            });
            result.Notes.Add(new Note() {
                Title = "Note_3",
                Category = category_B.Children.First(),
                Context = "Test Context"
            });
            result.Notes.Add(new Note() {
                Title = "Note_4",
                Category = category_B.Children.First(),
                Context = "Test Context"
            });
            result.Notes.Add(new Note() {
                Title = "Note_5",
                Category = category_B,
                Context = "Test Context"
            });
            result.Notes.Add(new Note() {
                Title = "Note_6",
                Category = category_B,
                Context = "Test Context"
            });

            result.SaveChanges();
            return result;
        }
    }
}
