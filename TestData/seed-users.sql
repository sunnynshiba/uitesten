DELETE FROM users;

-- Test Patient 1
INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
VALUES (
    'jjansen',
    'fgW/pr3lK9QFtmUdvKsR3rgW+R81PLzQNPJUh7Nd0Pk=',
    'jan.jansen@example.com',
    NOW(),
    4,
    '0612345671'
);

-- Test Patient 2
INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
VALUES (
    'pietp',
    'KgR12Xl0MGYj/gVeoTxBpgGrqmIt+bJ9xP855vCcSdk=',
    'pietp@example.com',
    NOW(),
    4,
    '0612345672'
);

-- Test Patient 3
INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
VALUES (
    'lisavd',
    'KgR12Xl0MGYj/gVeoTxBpgGrqmIt+bJ9xP855vCcSdk=',
    'vandam@example.com',
    NOW(),
    4,
    '0612345672'
);

-- Test Doctor (Huisarts)
INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
VALUES (
    'huisarts1',
    'gbwVEPsPR1Vsp8xDqDgFfQIT7ajvEgY14dPnOnr7dF0=',
    'huisarts1@example.com',
    NOW(),
    5,
    '0612345673'
);

-- Test Doctor (Specialist)
INSERT INTO users (Username, PasswordHash, Email, CreatedAt, UserRoleID, PhoneNumber)
VALUES (
    'spacialist1',
    'gbwVEPsPR1Vsp8xDqDgFfQIT7ajvEgY14dPnOnr7dF0=',
    'specialist1@example.com',
    NOW(),
    6,
    '0612345674'
);
