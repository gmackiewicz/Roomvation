# Roomvation
Roomvation is an ASP.NET MVC web application designed to keep track of conference room reservations.

### Assumptions
  - there is only one room in the system,
  - the administrator activates and locks users' accounts,
  - user needs an active account to be able to create a new reservation,
  - reservations can't overlap,
  - every reservation can be edited only before the meeting starts,
  - the creator can add other active users as meeting participants,
  - every reservation can be cancelled/deleted from the system at any time.

### Technology
 - ASP.NET MVC v5.2.3
 - Entity Framework v6.1.3 (Code First migrations)
 - Bootstrap CSS v4.0.0-alpha3
 - jQuery v3.1.1
