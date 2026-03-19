-- Table: fps.tblstagingmonthlyTime

CREATE TABLE fps."tblstagingmonthlyTime" (
    pactstaffid citext,
    timecode citext,
    parentproject citext,
    month double precision,
    workgroup citext,
    hours citext,
    failurecomments citext,
    passed boolean,
    pactid citext,
    newworkgroup citext,
    oldtestcode citext,
    name citext
);

