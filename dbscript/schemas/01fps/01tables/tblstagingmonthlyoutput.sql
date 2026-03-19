-- Table: fps.tblstagingmonthlyoutput

CREATE TABLE fps.tblstagingmonthlyoutput (
    testcode citext NOT NULL,
    buyer citext NOT NULL,
    month double precision NOT NULL,
    workgroup citext NOT NULL,
    volume double precision,
    failurecomments citext,
    passed boolean
);

