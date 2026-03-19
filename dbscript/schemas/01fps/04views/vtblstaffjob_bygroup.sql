-- View: fps.vtblstaffjob_bygroup

CREATE OR REPLACE VIEW fps.vtblstaffjob_bygroup AS
 SELECT sj.staffid,
    sj.jobcode,
    sj.plannedhours,
    sj.fpsyear
   FROM (fps.tblstaffjob sj
     JOIN fps.vtlkpproject_bygroup p ON (((p.parentproject = sj.jobcode) AND (p.fpsyear = sj.fpsyear))));
