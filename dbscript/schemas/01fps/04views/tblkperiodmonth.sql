-- View: fps.tblkperiodmonth

CREATE OR REPLACE VIEW fps.tblkperiodmonth AS
 SELECT tblperiodmonth.endmonth,
    tblperiodmonth.monthno,
    tblperiod.periodname,
    tblperiod.fpsyear
   FROM (fps.tblperiod
     JOIN fps.tblperiodmonth ON ((tblperiod.endperiod = tblperiodmonth.endmonth)));
