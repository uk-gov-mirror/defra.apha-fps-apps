-- View: fps.all_staff_project

CREATE OR REPLACE VIEW fps.all_staff_project AS
 SELECT tblstaffjob.staffid AS pactid,
    tblstaffjob.jobcode AS parentproject,
    tblstaffjob.fpsyear
   FROM fps.tblstaffjob
UNION
 SELECT monthlytime.pactstaffid AS pactid,
    monthlytime.parentproject,
    monthlytime.fpsyear
   FROM fps.monthlytime
  GROUP BY monthlytime.pactstaffid, monthlytime.parentproject, monthlytime.fpsyear;
