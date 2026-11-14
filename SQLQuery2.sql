SELECT 
    e.EnrolmentID, 
    e.ParticipantID, 
    u.FullName AS ParticipantName,
    e.CategoryID, 
    c.Name AS CategoryName, 
    c.EventID, 
    ev.Name AS EventName
FROM EventEnrolments e
JOIN Categories c ON e.CategoryID = c.CategoryID
JOIN Events ev ON c.EventID = ev.EventID
JOIN Users u ON e.ParticipantID = u.UserID;
