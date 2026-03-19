-- Table: fps.tblusers

CREATE TABLE fps.tblusers (
    user_id integer DEFAULT nextval('fps.tblusers_user_id_seq'::regclass) NOT NULL,
    username character varying(50),
    agencyid integer,
    frmwarning boolean DEFAULT false NOT NULL,
    comments character varying(255),
    dt2username character varying(50),
    useremail character varying(255),
    CONSTRAINT pk__tblusers__1367e606 PRIMARY KEY (user_id),
    CONSTRAINT username UNIQUE (username)
);

