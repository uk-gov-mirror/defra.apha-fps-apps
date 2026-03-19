-- Table: fps.tblbid

CREATE TABLE fps.tblbid (
    workgroup citext NOT NULL,
    account citext NOT NULL,
    genbid money DEFAULT 0 NOT NULL,
    fpsyear integer NOT NULL,
    CONSTRAINT pk_tblbid PRIMARY KEY (workgroup, account, fpsyear),
    CONSTRAINT fk_tblbid_account FOREIGN KEY (account, fpsyear) REFERENCES fps.tblkpaccountcategory(accshortname, fpsyear),
    CONSTRAINT fk_tblbid_workgroup FOREIGN KEY (workgroup, fpsyear) REFERENCES fps.workgroup(workgroup, fpsyear),

    CONSTRAINT fk_tblbid_fpsyear FOREIGN KEY (fpsyear) REFERENCES fps.tblyearmaster(fpsyear)
);
