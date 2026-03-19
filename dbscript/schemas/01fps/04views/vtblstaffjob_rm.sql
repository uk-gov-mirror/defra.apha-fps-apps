-- View: fps.vtblstaffjob_rm

CREATE OR REPLACE VIEW fps.vtblstaffjob_rm AS
 SELECT sj.staffid,
    sj.jobcode,
    sj.plannedhours,
    sj.fpsyear
   FROM (fps.tblstaffjob sj
     JOIN fps.vtblwgemployee we ON (((we.pactid = sj.staffid) AND (we.fpsyear = sj.fpsyear))));
