# GroupFourTaskMVC
Task for Group Four to create an online web application for reserving books.

The home page fulfills all the required functionality. You can search for books, reserve books, and cancel reservations (not listed in
requirements, but without that option once a book is reserved it's stuck that way).
Reservation events are stored in a flat file, and the reservation status file is periodically updated to account for all events. 
As the application and data are both small, tasks are not triggered asynchronously. 

No user login has been implemented as I felt it was out of scope. As such, anyone can reserve/cancel books. 
In order to implement customer tracking to control who could cancel reservations, either login information would need to be stored or
the user would have to write down their reservation number as use that as a passcode to cancel their order. As the reservation number
is a guid, this would also be a poor user experience. Alternatives are using sequential numbers for reservation booking, or storing
just a client name for the reservation that the client has to enter to cancel.

Databases weren't used as storage as there were only a few books and few relationships between the different objects. A json file
was used for the book data, as it was static. Flat files were used for reservation events and the reservation status snapshot. 
