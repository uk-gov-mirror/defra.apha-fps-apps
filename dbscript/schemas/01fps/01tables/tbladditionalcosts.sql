-- Table: fps.tbladditionalcosts

CREATE TABLE fps.tbladditionalcosts (
    jobcode citext NOT NULL,
    account citext NOT NULL,
    description character varying(20) NOT NULL,
    itemcost money DEFAULT 0 NOT NULL,
    freq character varying(5),
    supplier character varying(50),
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tbladditionalcosts PRIMARY KEY (jobcode, account, description, fpsyear),
    CONSTRAINT fk_tbladditionalcosts_account FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory(accshortname, fpsyear),
    CONSTRAINT fk_tbladditionalcosts_jobcode FOREIGN KEY (jobcode, fpsyear) REFERENCES fps.tlkpproject(parentproject, fpsyear),

    CONSTRAINT fk_tbladditionalcosts_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
