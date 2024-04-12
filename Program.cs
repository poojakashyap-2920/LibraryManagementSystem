using System;
using System.Collections.Generic;
using System.Threading;

public class Book
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }

    public Book(string title, string author, string isbn)
    {
        Title = title;
        Author = author;
        ISBN = isbn;
    }
}

public class User
{
    public string UserName { get; set; }
    public int UserID { get; set; }

    public User(string name, int id)
    {
        UserName = name;
        UserID = id;
    }
}

public class Library
{
    private List<Book> books;
    private Dictionary<User, List<Book>> booksBorrowed;
    private object lockObj = new object();

    public Library()
    {
        books = new List<Book>();
        booksBorrowed = new Dictionary<User, List<Book>>();
    }

    public void AddBook(Book book)
    {
        books.Add(book);
    }

    public void DisplayBooks()
    {
        Console.WriteLine("Available Books:");
        foreach (var book in books)
        {
            Console.WriteLine($"Title: {book.Title}, Author: {book.Author}, ISBN: {book.ISBN}");
        }
    }

    public void BorrowBook(User user, string title)
    {
        Thread t1 = new Thread(() =>
        {
            lock (lockObj)
            {
                Book bookToBorrow = books.Find(book => book.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (bookToBorrow != null)
                {
                    books.Remove(bookToBorrow);
                    if (!booksBorrowed.ContainsKey(user))
                    {
                        booksBorrowed[user] = new List<Book>();
                    }
                    booksBorrowed[user].Add(bookToBorrow);
                    Console.WriteLine($"{user.UserName} borrowed {bookToBorrow.Title}");
                }
                else
                {
                    Console.WriteLine($"Book '{title}' is not available.");
                }
            }
        });
        t1.Start();
        t1.Join();
    }

    public void ReturnBook(User user, string title)
    {
        Thread t2 = new Thread(() =>
        {
            lock (lockObj)
            {
                User existingUser = booksBorrowed.Keys.FirstOrDefault(u => u.UserID == user.UserID);
                if (existingUser != null)
                {
                    Book returnedBook = booksBorrowed[existingUser].Find(book => book.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
                    if (returnedBook != null)
                    {
                        booksBorrowed[existingUser].Remove(returnedBook);
                        books.Add(returnedBook);
                        Console.WriteLine($"{existingUser.UserName} returned {returnedBook.Title}");
                    }
                    else
                    {
                        Console.WriteLine($"You did not borrow '{title}'");
                    }
                }
                else
                {
                    Console.WriteLine($"{user.UserName} has no books to return.");
                }
            }
        });
        t2.Start();
        t2.Join();
    }

}

class Program
{
    static void Main(string[] args)
    {
        Library library = new Library();
        library.AddBook(new Book("Pride and Prejudice", "Jane Austen", "9781593275846"));
        library.AddBook(new Book("To Kill a Mockingbird", "Harper Lee", "9780061120084"));

        Console.WriteLine("Welcome to the Library!");

        while (true)
        {
            Console.WriteLine("\n1. Display Available Books");
            Console.WriteLine("2. Borrow a Book");
            Console.WriteLine("3. Return a Book");
            Console.WriteLine("4. Exit");

            Console.Write("\nEnter your choice: ");
            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    library.DisplayBooks();
                    break;
                case 2:
                    Console.Write("Enter your name: ");
                    string userName = Console.ReadLine();
                    Console.Write("Enter the title of the book you want to borrow: ");
                    string bookToBorrow = Console.ReadLine();
                    library.BorrowBook(new User(userName, 0), bookToBorrow);
                    //Console.WriteLine($"{userName} borrowed {bookToBorrow}");
                    break;
                case 3:
                    Console.Write("Enter your name: ");
                    string userNameReturn = Console.ReadLine();
                    Console.Write("Enter the title of the book you want to return: ");
                    string bookToReturn = Console.ReadLine();
                    library.ReturnBook(new User(userNameReturn, 0), bookToReturn);
                    break;
                case 4:
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }
}