-- View: fps.vtlkpproject_general

CREATE OR REPLACE VIEW fps.vtlkpproject_general AS
 SELECT parentproject,
    projecttitle,
    shorttitle,
    program,
    projectstatus,
    customer,
    manager,
    disease,
    contract,
    datecreated,
    isdefraproject,
    projectgroup,
    fpsyear
   FROM fps.tlkpproject;
