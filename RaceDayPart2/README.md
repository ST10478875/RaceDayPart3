# RaceDay – Part 3

## Project Overview
RaceDay is a web-based race event management system developed using ASP.NET Core MVC and ASP.NET Core Web API.

The application allows participants to browse sporting events, enter races, view their enrolments and results, and manage their profiles. Organisers can manage events and race categories.

## Solution Structure

The solution contains two projects:

- **RaceDayMVC:** The ASP.NET Core MVC frontend.
- **RaceDayPart2:** The ASP.NET Core Web API backend.

## Main Features

### Authentication and Authorisation
- User registration and login.
- Participant and Organiser roles.
- Role-based navigation and access control.
- Session management and logout.

### Organiser Features
- Organiser dashboard.
- Create, view, edit and delete events.
- Manage event categories.

### Participant Features
- Browse available events.
- Enter events.
- View enrolments and results.
- Manage participant profiles.

### Azure Blob Storage
- Upload profile pictures through the API.
- Store uploaded images in Azure Blob Storage.
- Save image URLs in the database.

## Technologies Used
- C#
- ASP.NET Core MVC
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server LocalDB
- Azure Blob Storage
- Swagger / OpenAPI
- Visual Studio 2022

## Running the Application

1. Open the RaceDay solution in Visual Studio 2022.
2. Configure the API database connection.
3. Configure the Azure Blob Storage connection securely using User Secrets.
4. Start the `RaceDayPart2` API project.
5. Start the `RaceDayMVC` project.
6. Access the MVC application at `https://localhost:7095/`.
7. Access the API at `https://localhost:7207/`.

## Security
Sensitive configuration values, including Azure Storage connection strings, should not be committed to GitHub. Use User Secrets or environment variables.

## Docker
Docker build and run instructions will be added after the Docker configuration is completed.

## CI/CD
The GitHub Actions workflow and build verification evidence will be added here.

## Video Demonstration
https://youtu.be/eZMsq9Qu2_s