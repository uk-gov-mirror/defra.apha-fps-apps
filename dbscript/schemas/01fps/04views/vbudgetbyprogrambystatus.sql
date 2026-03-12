-- View: fps.vbudgetbyprogrambystatus

CREATE OR REPLACE VIEW fps.vbudgetbyprogrambystatus AS
 SELECT program,
    projectstatus,
    fpsyear,
    sum(budget_cvl) AS statusbudget
   FROM fps.tlkpproject
  GROUP BY program, projectstatus, fpsyear;
