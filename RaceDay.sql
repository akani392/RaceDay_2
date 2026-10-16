-- start of database setup

create database RaceDay_Project;
use RaceDay_Project;


-- start of create table for roles
-- this table acts as a lookup for the two main system roles: organisers and participants
create table Roles (
    RoleID int identity(1,1) primary key,
    RoleName varchar(20) not null unique
);
-- end of create table for roles


-- start of create table for users
-- this table stores all user profiles and links them to a specific role 
create table Users(
UserID int identity(1,1) primary key,
RoleID int not null,
FirstName varchar(50) not null,
LastName varchar(50) not null,
Email varchar(100) not null unique,
PasswordHash varchar(255) not null,
DateRegistered datetime not null default getdate(),

constraint fk_user_role foreign key (RoleID)
references Roles(RoleID)
);
-- end of create table for users


-- start of create table for events
-- this table stores the main racing events, which are created and managed by an organiser
create table Event (
EventID int identity(1,1) primary key,
OrganiserID int not null,
EventName varchar(100) not null,
EventDate date not null,
EventType varchar(20) not null

constraint ck_event_eventtype
check (EventType in ('Running', 'Walking', 'Cycling')),
Description varchar(500) null,
Location varchar(150) not null,

constraint fk_event_organiser foreign key (OrganiserID) 
references Users(UserID)
);
-- end of create table for events


-- start of create table for categories
-- this table breaks down an event into different distances (e.g., 10km, 21km) and sets participant limits
create table Category (
    CategoryID int identity(1,1) primary key,
    EventID int not null,
    CategoryName varchar(50) not null,
    DistanceKM decimal(5,2) not null
        constraint ck_category_distancekm check (DistanceKM > 0),
    MaxParticipants int not null
        constraint ck_category_maxparticipants check (MaxParticipants > 0),

    constraint fk_category_event foreign key (EventID)
        references Event(EventID)
);
-- end of create table for categories


-- start of create table for enrolments
-- this table links participants to the specific categories they have signed up for
create table Enrolment (
    EnrolmentID int identity(1,1) primary key,
    ParticipantID int not null,
    CategoryID int not null,
    EnrolmentDate datetime not null default getdate(),
    Status varchar(20) not null default 'Confirmed'
        constraint ck_enrolment_status
        check (Status in ('Confirmed', 'Cancelled')),

    constraint fk_enrolment_participant foreign key (ParticipantID)
        references Users(UserID),
    constraint fk_enrolment_category foreign key (CategoryID)
        references Category(CategoryID),

    -- prevents the same person from signing up for the exact same category twice
    constraint uq_enrolment_participant_category
        unique (ParticipantID, CategoryID)
);
-- end of create table for enrolments


-- start of create table for results
-- this table records the finishing time and position for a completed enrolment
create table Result (
    ResultID int identity(1,1) primary key,
    EnrolmentID int not null unique,
    CapturedByID int not null,
    FinishTime time not null,
    Position int not null
        constraint ck_result_position check (Position > 0),
    DateCaptured datetime not null default getdate(),

    constraint fk_result_enrolment foreign key (EnrolmentID)
        references Enrolment(EnrolmentID),
    constraint fk_result_capturedby foreign key (CapturedByID)
        references Users(UserID)
);
-- end of create table for results


-- start of verification queries
-- select statements to view the newly created and populated tables
select * from Roles;
select * from Users;
select * from Event;
select * from Category;
select * from Enrolment;
select * from Result;

-- start of seed data insertions
-- insert base roles
insert into Roles (RoleName) values
('Organiser'),
('Participant');

-- insert sample users (2 organisers, 2 participants)
insert into Users (RoleID, FirstName, LastName, Email, PasswordHash) values
(1, 'Thabo', 'Nkosi', 'thabo.nkosi@raceday.co.za', 'hashed_pw_001'),
(1, 'Michelle', 'van Wyk', 'michelle.vanwyk@raceday.co.za', 'hashed_pw_002'),
(2, 'Sipho', 'Dlamini', 'sipho.dlamini@example.com', 'hashed_pw_003'),
(2, 'Amanda', 'Botha', 'amanda.botha@example.com', 'hashed_pw_004');

-- insert sample events
insert into Event (OrganiserID, EventName, EventDate, EventType, Description, Location) values
(1, 'Soweto Marathon', '2026-11-08', 'Running', 'Annual road running event through the streets of Soweto.', 'Soweto, Johannesburg'),
(2, 'Cape Town Cycle Tour', '2026-03-08', 'Cycling', 'Scenic cycling race around the Cape Peninsula.', 'Cape Town'),
(1, 'Sunday Park Run & Walk', '2026-09-20', 'Walking', 'Community fun walk supporting local charities.', 'Delta Park, Johannesburg');

-- insert sample categories for the events
insert into Category (EventID, CategoryName, DistanceKM, MaxParticipants) values
(1, '10km', 10.00, 500),
(1, '21km Half Marathon', 21.10, 300),
(1, '42km Full Marathon', 42.20, 150),
(2, '109km Cycle Route', 109.00, 1000),
(2, '56km Cycle Route', 56.00, 800),
(3, '5km Fun Walk', 5.00, 200),
(3, '10km Walk', 10.00, 150);

-- insert sample enrolments
insert into Enrolment (ParticipantID, CategoryID, Status) values
(3, 1, 'Confirmed'),
(3, 4, 'Confirmed'),
(4, 2, 'Confirmed'),
(4, 6, 'Cancelled');

-- insert sample results for confirmed enrolments
insert into Result (EnrolmentID, CapturedByID, FinishTime, Position) values
(1, 1, '00:52:31', 45),
(3, 1, '01:58:12', 112);
-- end of seed data insertions



