RaceDay_2
RaceDay_2 is an ASP.NET Core API for organizing and entering race events. Organizers are able to create events and categories, view who has entered and capture results. Participants can look through events, enter a category and view their own enrolments and results. The API make use of Entity Framework Core against a SQL Server database, cookie-based login with server-side role checks, hashed passwords and Swagger UI for testing 
How logging in works
o	After POST /api/Authentication/login, the API stores your user ID and role in a secure cookie. Every later request in that browser session uses the cookie, so you do not log in again for each call.
o	Swagger UI keeps cookies automatically. Only one person can be logged in per browser session. To switch between Organiser and Participant, call POST /api/Authentication/logout first, then log in as the other user.
o	Passwords are stored as salted PBKDF2 hashes and are never returned by the API.
o	If you are not logged in you get 401. If you are logged in with the wrong role you get 403. If you are the right role but the data belongs to someone else, you also get 403.
The users in RaceDay.sql cannot log in. Their passwords are placeholder text, not real hashes. Register new users through the API instead.

Using Swagger
For each call: click the endpoint, press Try it out, fill in the fields, press Execute, and read the Server response.
o	Most inputs are typed into fields as query values.
o	PUT /api/User_Profile/UpdateUser takes a JSON body.
o	finishTime is written hh:mm:ss, for example 01:45:30.
o	eventType must be exactly Running, Walking or Cycling.
o	eventDate is written like 2026-12-01.
 
Walkthrough: Organiser
1.	Register: POST /api/Authentication/register with firstName, lastName, email, password, roleId = 1. You get 201.
2.	Log in: POST /api/Authentication/login with that email and password. You get "Login Successful".
3.	Create an event: POST /api/Events with eventName, eventDate, location, description, eventType. Note the eventID in the response.
4.	Add categories: POST /api/events/{eventId}/categories with categoryName (e.g. 10km), distanceKm, maxParticipants. Add as many as you need.
5.	Manage your events
o	Edit an event: PUT /api/Events/{id}
o	Delete an event: DELETE /api/Events/{id}. All its categories must be removed first.
o	Edit a category: PUT /api/categories/{id}
o	Delete a category: DELETE /api/categories/{id}
6.	See who has entered: GET /api/events/{eventId}/enrolments lists every enrolment across all categories of your event.
7.	Capture a result after the race: POST /api/enrolments/{enrolmentId}/result with finishTime and position. Each enrolment can have one result.
8.	Correct a result: PUT /api/results/{id} with the new finishTime and position. Only the organiser who captured it can correct it.
9.	Your profile: GET /api/User_Profile/GetUser, PUT /api/User_Profile/UpdateUser.
10.	Log out: POST /api/Authentication/logout.
An Organiser can only change events, categories, enrolment lists and results that belong to their own events. Other organizers’ events return 403.
 
Walkthrough: Participant
1.	Register: POST /api/Authentication/register with roleId = 2.
2.	Log in: POST /api/Authentication/login.
3.	Browse events: GET /api/Events lists all events. GET /api/Events/{id} shows one event with its categories.
4.	See an event's categories: GET /api/events/{eventId}/categories.
5.	Enroll : POST /api/enrolments with categoryId. You get 201 with your enrolment.
o	409 means the category is full or you are already enrolled in it.
6.	View your enrolments: GET /api/enrolments/me.
7.	Cancel an enrolment: DELETE /api/enrolments/{id}. You can only cancel your own.
8.	View a result: GET /api/enrolments/{enrolmentId}/result for one enrolment, or GET /api/users/me/results for your full results history (event, category, time, position).
9.	Your profile: GET /api/User_Profile/GetUser, PUT /api/User_Profile/UpdateUser.
10.	Log out: POST /api/Authentication/logout.
A Participant cannot create events, categories or results. Those endpoints return 403.
 
Endpoint reference
Method	Route	Who	Purpose
Post	/api/Authentication/register	Anyone	Create an account
Post	/api/Authentication/login	Anyone	Log in and start the session
Post	/api/Authentication/logout	Anyone	End the session
Get	/api/User_Profile/GetUser	Any logged in user	View your profile
Put	/api/User_Profile/UpdateUser	Any logged in user	Update your name and email
Get	/api/Events
	Any logged in user	List events
Get	/api/Events/{id}	Any logged in user	One event with its categories
Post	/api/Events	Organizer	Create an event
Put	/api/Events/{id}/	Organizer (owner)	Update an event
Delete	/api/Events/{id}	Organizer (owner)	Delete an event
Get	/api/events/{eventId}/categories	Any logged in user	List an event’s categories
Post	/api/events/{eventId}/categories	Organizer (owner)	Add a category
Put	/api/categories/{id}	Organizer (owner)	Update a category
Delete	/api/categories/{id}	Organizer (owner)	Delete a category
Post	/api/enrolments	Participant	Enrol in a category
Get	/api/enrolments/me	Participant	Your enrolments
Get	/api/events/{eventId}/enrolments	Organizer (owner)	All enrolments for an event
Delete	/api/enrolments/{id}	Participant (owner)	Cancel an enrolment
Post	/api/enrolments/{enrolmentId}/result	Organiser (owner of the event)	Capture a result
Get	/api/enrolments/{enrolmentId}/result	Any logged in user	View a result
Put	/api/results/{id}	Organiser (who captured it)	Correct a result
get	/api/users/me/results	Participant	Your results history

Status codes you will see: 200 OK, 201 created, 204 deleted, 400 invalid input, 401 not logged in, 403 wrong role or not your data, 404 not found, 409 conflict (full category, already enrolled, result already captured, event still has categories)
