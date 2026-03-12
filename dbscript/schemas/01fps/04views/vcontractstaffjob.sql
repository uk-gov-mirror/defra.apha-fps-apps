-- View: fps.vcontractstaffjob

CREATE OR REPLACE VIEW fps.vcontractstaffjob AS
 SELECT sj.staffid,
    sj.jobcode,
    sj.plannedhours,
    sj.fpsyear
   FROM fps.tblstaffjob sj
   JOIN fps.vcontractproject p ON p.parentproject = sj.jobcode AND p.fpsyear = sj.fpsyear;
