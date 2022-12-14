--Find a exisitng user id and use it as the CreatedById, LastUpdatedBy. Get the correct values for customer names
--select * from "Users"

insert into "Users" 
("Id", "Email", "FirstName", "LastName", "CreatedById", "CreatedDateUtc", "LastUpdatedById", "LastUpdatedDateUtc", "ConcurrencyKey")
values
('a336b227-644b-44e8-b6f1-6b9261a3fb74', 'wijithapaw@gmail.com', 'Wijtiha', 'Wijenayake', 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74');


insert into "Customers" 
("Id", "Name", "Inactive", "CreatedById", "CreatedDateUtc", "LastUpdatedById", "LastUpdatedDateUtc", "ConcurrencyKey")
values
('a336b227-644b-44e8-b6f1-6b9261a3fb80', 'JKCS', false, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fb81', 'EXPO', false, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fb82', 'ABC', false, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fb83', 'XYZ', false, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fb84', 'PQR', false, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fb85', 'STU', false, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74');

--select * from "Customers"

insert into "Products" 
("Id", "Code", "Inactive", "SortOrder", "CreatedById", "CreatedDateUtc", "LastUpdatedById", "LastUpdatedDateUtc", "ConcurrencyKey")
values
('a336b227-644b-44e8-b6f1-6b9261a3fc80', 'MGO', false, 10, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fc81', '380_LSFO', false, 20, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74'),
('a336b227-644b-44e8-b6f1-6b9261a3fc82', '380_HSFO', false, 30, 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74', now(), 'a336b227-644b-44e8-b6f1-6b9261a3fb74');

--select * from "Products"
