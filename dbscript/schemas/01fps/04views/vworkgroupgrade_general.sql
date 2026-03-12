-- View: fps.vworkgroupgrade_general

CREATE OR REPLACE VIEW fps.vworkgroupgrade_general AS
 SELECT wggrade,
    profitcentregrade,
    gradecode,
    workgroup,
    fpsyear
   FROM fps.workgroupgrade;
