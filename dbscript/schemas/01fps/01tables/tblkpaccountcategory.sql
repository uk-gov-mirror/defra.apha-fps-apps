-- Table: fps.tblkpaccountcategory

CREATE TABLE fps.tblkpaccountcategory (
    accshortname citext NOT NULL,
    accountdescription character varying(50),
    constituentaccountcodes character varying(100),
    accounttype citext NOT NULL,
    projectspecific integer,
    rcspecific integer,
    csg7_group character(15),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblkpaccountcategory PRIMARY KEY (accshortname, fpsyear),
    CONSTRAINT tblkpaccountcategory_ck_accounttype CHECK (accounttype = 'Pay'::citext OR accounttype = 'NPRC'::citext),

    CONSTRAINT fk_tblkpaccountcategory_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
