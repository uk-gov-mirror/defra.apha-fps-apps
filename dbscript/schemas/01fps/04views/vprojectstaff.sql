-- View: fps.vprojectstaff

CREATE OR REPLACE VIEW fps.vprojectstaff AS
 SELECT DISTINCT tblstaffjob.jobcode AS project,
    tblstaffjob.staffid,
    tblstaffjob.fpsyear
   FROM fps.tblstaffjob
UNION
 SELECT DISTINCT monthlytime.parentproject AS project,
    monthlytime.pactstaffid AS staffid,
    monthlytime.fpsyear
   FROM fps.monthlytime;
