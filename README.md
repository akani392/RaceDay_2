What this project does
RaceDay_2 is a REST API for organising and entering race events (running, walking and cycling). It has two kinds of user:
o	Organisers create events, split them into distance categories (for example 10km and 21km), see who has enrolled and capture each runner’s finish time and position.
o	Participants browse events, enroll in a category, cancel an enrolment and view their own enrolments and results.
Passwords are stored as salted PBKDF2 hashes, login uses a secure cookie, and every endpoint checks the user’s role on the server. Swagger UI is included so the whole API can be tested from the browser.
Technology used
o	ASP.NET Core 8 Web API (controllers)
o	Entity Framework Core 8 with SQL Server (LocalDB instance RaceDay_POE, database RaceDay_Project)
o	Cookie authentication with role based [Authorize] attributes
o	Swagger / Swashbuckle for interactive testing
o	xUnit and the EF Core In-Memory provider for unit tests 
Project structure
File	What it does
Program.cs	Starts the app: registers controllers, the database, cookie login (401/403 instead of redirects) and Swagger.
appsettings.json	Holds the database connection string (RaceDayConnection).
Models/	Entity classes that match the database tables: User, Roles, Event, Category, Enrolment and Result. Raceday_Context.cs is the EF Core DbContext that maps them and sets the relationships and constraints.
Models/Update_Profile_Request.cs	Request body used when a user edits their own profile.
Controllers/Authentication.cs	Register, login and logout.
Controllers/Password_Helper.cs	Hashes and verifies passwords (PBKDF2, SHA-256, random salt).
Controllers/User_Profile.cs	View and update the logged in user’s own profile.
Controllers/Events.cs	List, view, create, update and delete events.
Controllers/Categories.cs	List, add, update and delete the categories of an event.
Controllers/Event_enrolments.cs	Enrol, list enrolments and cancel an enrolment.
Controllers/Results.cs	Capture, view and correct race results.
RaceDay_2_Test/	xUnit test project (see section 11).
RaceDay.sql	Creates the database, tables and sample data.

Every controller follows the same pattern: check the input, check the role (attribute), check that the data belongs to the logged in user, do the database work, and return a clear status code. The data is never returned as raw entities. Each action returns only the fields it needs, so password hashes never leave the API.
 
Getting started
Requirements
o	Visual Studio 2022 or later with the ASP.NET workload, or the .NET 8 SDK
o	SQL Server LocalDB (installed with Visual Studio) and a tool to run SQL such as SSMS or Visual Studio’s SQL Server Object Explorer
Set up the database
1.	Create and start the LocalDB instance. In a terminal run: sqllocaldb create RaceDay_POE then sqllocaldb start RaceDay_POE
2.	Connect to (localdb)\RaceDay_POE and run RaceDay.sql. It creates the RaceDay_Project database, the six tables, and inserts the two roles (Organiser and Participant) plus some sample data.
3.	Check it worked: select * from Roles; must show Organiser (RoleID 1) and Participant (RoleID 2).
4.	Open appsettings.json and make sure RaceDayConnection points at your instance. The default is (localdb)\RaceDay_POE with Initial Catalog=RaceDay_Project.
Run the API
o	Open RaceDay_2.slnx in Visual Studio.
o	Choose the https launch profile and press F5 (or run dotnet run --launch-profile https).
o	Swagger opens at https://localhost:7016/swagger. All endpoints are listed there.
How logging in works
o	After POST /api/Authentication/login the API creates a secure cookie (RaceDay.Auth) that stores your user ID, email and role. It lasts 8 hours and every later request in the same browser uses it automatically.
o	Only one person can be logged in per browser session. To switch between an Organiser and a Participant, call logout first, then log in as the other user (or use a second browser or a private window).
o	Not logged in: 401. Logged in with the wrong role: 403. Right role but the data belongs to someone else: 403.
o	Passwords are never stored or returned in plain text.
Using Swagger
For every call: click the endpoint, press Try it out, fill in the fields, press Execute and read the Server response.
o	Most inputs are typed into fields as query values. Only PUT /api/User_Profile/UpdateUser takes a JSON body.
o	Dates are written like 2026-12-01 and finish times as hh:mm:ss, for example 01:45:30.
o	eventType must be exactly Running, Walking or Cycling.
o	roleId when registering: 1 = Organiser, 2 = Participant.

Walkthrough: Organiser
Follow these steps in order. IDs in the examples will be different in your database. Use the ones the API returns.
1.	Register. POST /api/Authentication/register with firstName, lastName, email, password and roleId = 1. You get 201 Created.
2.	Log in. POST /api/Authentication/login with the same email and password. You get “Login Successful” and your RoleName is Organiser.
3.	Create an event. POST /api/Events with eventName (Soweto Marathon), eventDate (2026-12-01), location, description and eventType (Running). Write down the eventID in the response.
4.	Add categories. POST /api/events/{eventId}/categories with categoryName (10km), distanceKm (10) and maxParticipants (100). Repeat for each distance. Write down each categoryID.
5.	Check your event. GET /api/Events/{id} shows the event with its categories.
6.	Edit or remove. PUT /api/Events/{id} and PUT /api/categories/{id} change details. DELETE /api/categories/{id} removes a category. DELETE /api/Events/{id} removes an event, but only once all of its categories have been removed (otherwise 409).
7.	See who has entered. GET /api/events/{eventId}/enrolments lists every enrolment across all categories of your event.
8.	Capture a result after the race. POST /api/enrolments/{enrolmentId}/result with finishTime (01:45:30) and position (1). Each enrolment can have only one result (a second one gives 409).
9.	Correct a result. PUT /api/results/{id} with the new finishTime and position. Only the organiser who captured it can correct it.
10.	Your profile. GET /api/User_Profile/GetUser and PUT /api/User_Profile/UpdateUser (JSON body with firstName, lastName and email).
11.	Log out. POST /api/Authentication/logout.
Walkthrough: Participant
The organiser must already have created an event with at least one category (section 7) before a participant can enroll.
1.	Register. POST/api/Authentication/register with roleId = 2. You get 201 Created.
2.	Log in. POST /api/Authentication/login. RoleName is Participant.
3.	Browse events. GET /api/Events lists all events. GET /api/Events/{id} shows one event with its categories.
4.	Pick a category. GET /api/events/{eventId}/categories shows the distances available. Note the categoryID you want.
5.	Enroll. POST /api/enrolments with categoryId. You get 201 with your enrolment. A 409 means the category is full or you are already enrolled in it.
6.	View your enrolments. GET /api/enrolments/me (you only ever see your own).
7.	Cancel an enrolment. DELETE /api/enrolments/{id}. You can only cancel your own.
8.	View results after the race. GET /api/enrolments/{enrolmentId}/result for one enrolment, or GET /api/users/me/results for your full history (event, category, time and position).
9.	Your profile. GET /api/User_Profile/GetUser and PUT /api/User_Profile/UpdateUser.
10.	Log out. POST /api/Authentication/logout.
 
Who can do what
Action	Logged out	Participant	Organiser
Register, login, logout	Allowed	Allowed	Allowed
View or edit own profile	401	Allowed	Allowed
Browse events and categories	401	Allowed	Allowed
Create, edit, delete events and categories	401	403	Own events only
Enrol, view and cancel own enrolments	401	Allowed	403
View all enrolments for an event	401	403	Own events only
Capture or correct results	401	403	Own events only
View a result for an enrolment	401	Allowed	Allowed
View own results history	401	Allowed	403
Endpoint reference
Method	Route	Who	Purpose
POST	/api/Authentication/register	Anyone	Create an account
POST	/api/Authentication/login	Anyone	Log in and start the session
POST	/api/Authentication/logout	Anyone	End the session
GET	/api/User_Profile/GetUser	Any logged in user	View your profile
PUT	/api/User_Profile/UpdateUser	Any logged in user	Update name and email
GET	/api/Events	Any logged in user	List events
GET	/api/Events/{id}	Any logged in user	One event with its categories
POST	/api/Events	Organiser	Create an event
PUT	/api/Events/{id}	Organiser (owner)	Update an event
DELETE	/api/Events/{id}	Organiser (owner)	Delete an event
GET	/api/events/{eventId}/categories	Any logged in user	List an event’s categories
POST	/api/events/{eventId}/categories	Organiser (owner)	Add a category
PUT	/api/categories/{id}	Organiser (owner)	Update a category
DELETE	/api/categories/{id}	Organiser (owner)	Delete a category
POST	/api/enrolments	Participant	Enrol in a category
GET	/api/enrolments/me	Participant	Your enrolments
GET	/api/events/{eventId}/enrolments	Organiser (owner)	All enrolments for an event
DELETE	/api/enrolments/{id}	Participant (owner)	Cancel an enrolment
POST	/api/enrolments/{enrolmentId}/result	Organiser (owner)	Capture a result
GET	/api/enrolments/{enrolmentId}/result	Any logged in user	View a result
PUT	/api/results/{id}	Organiser (captured it)	Correct a result
GET	/api/users/me/results	Participant	Your results history

Status codes
Code	Meaning
200 / 201 / 204	OK / Created / Deleted (no content)
400	Invalid input, for example a wrong eventType, a distance or position of 0 or less, or a duplicate email
401	Not logged in
403	Wrong role, or the data belongs to someone else
404	The event, category, enrolment or result does not exist
409	Category full, already enrolled, result already captured, or event still has categories
Unit tests
The RaceDay_2_Test project contains 46 xUnit tests. Each test builds a fresh in-memory database, adds the roles and any users, events or categories it needs, calls a controller method directly and asserts on the result and on what was saved.
Test file	What it proves
Test_Helper.cs	Shared setup: creates the in-memory database, adds sample data, and fakes a logged in user (by setting the same claims that login creates) or a fake sign-in service.
Testing_register_login.cs	Register works for both roles, rejects a bad role or duplicate email, stores only a hash; login succeeds or fails correctly and puts the user ID and role in the session; logout works.
Testing_Events.cs	Organisers can create, update and delete their own events; invalid types, unknown IDs, other people’s events and events with categories are refused.
Testing_enrolments.cs	Participants can enrol and cancel; full categories, duplicates and other people’s enrolments are refused; people only see their own data.
Testing_Role_Access.cs	Reads the [Authorize] attribute on each method to prove the role rules, and checks that one organiser cannot touch another organiser’s data.
