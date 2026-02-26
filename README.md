📝 Blog Management System
A Secure ASP.NET Core MVC Blogging Platform with Authentication & Role-Based Access

📌 Introduction

Blog Management System is a full-stack web application built using ASP.NET Core MVC, MySQL, and Bootstrap.
The platform provides secure user authentication with role-based authorization, allowing:
🛠️ Admins to manage blog content (Create, Edit, Delete, Update)
👤 Registered users to read, like, and comment on blog posts
🔐 Secure login system to protect interactions

This project demonstrates modern web development practices including MVC architecture, authentication, authorization, relational database integration, and responsive UI design.

🚀 Features

👨‍💼 Admin Panel
Create blog posts
Edit existing posts
Delete posts
Manage users (optional if implemented)
Role-based access control

👤 User Features
User Registration & Login
Read blog posts
Like posts (after login)
Comment on posts (after login)

🔐 Security Features
Authentication system
Role-based authorization
Secure password handling
Protected routes for admin actions

🎨 UI Features
Responsive design with Bootstrap
Clean and modern layout
Mobile-friendly interface

🛠 Tech Stack
Technology	Purpose
ASP.NET Core MVC	Backend Framework
MySQL	Relational Database
Entity Framework Core	ORM
Bootstrap	Frontend Styling
ASP.NET Identity	Authentication & Authorization
🏗 Architecture

The project follows the MVC (Model-View-Controller) architectural pattern:
Models → Represent database entities (User, Blog, Comment, Like)
Views → Razor views for UI rendering
Controllers → Handle HTTP requests and application logic

This ensures:
Clean separation of concerns
Maintainable code structure
Scalability
