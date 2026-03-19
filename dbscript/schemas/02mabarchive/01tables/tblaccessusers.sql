-- Table: mabarchive.tblaccessusers

CREATE TABLE mabarchive.tblaccessusers (
    systemid integer NOT NULL,
    ntlogin character varying(50) NOT NULL,
    username character varying(50),
    dt2login character varying(50),
    useremail character varying(255),
    CONSTRAINT pk_tblaccessusers PRIMARY KEY (systemid, ntlogin)
);

