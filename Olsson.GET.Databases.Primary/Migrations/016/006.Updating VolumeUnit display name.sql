alter table dbo.VolumeUnit add VolumeUnitPluralizedName varchar(50) null
go

update dbo.VolumeUnit set VolumeUnitPluralizedName = 'Acre-Feet', VolumeUnitDisplayName = 'Acre-Feet' where VolumeUnitID = 1
update dbo.VolumeUnit set VolumeUnitPluralizedName = 'Cubic Feet', VolumeUnitDisplayName = 'Cubic Feet' where VolumeUnitID = 2
update dbo.VolumeUnit set VolumeUnitPluralizedName = 'Cubic Yards', VolumeUnitDisplayName = 'Cubic Yard' where VolumeUnitID = 3
update dbo.VolumeUnit set VolumeUnitPluralizedName = 'Cubic Meters', VolumeUnitDisplayName = 'Cubic Meter' where VolumeUnitID = 4
update dbo.VolumeUnit set VolumeUnitPluralizedName = 'Gallons', VolumeUnitDisplayName = 'Gallon', VolumeUnitName = 'Gallon' where VolumeUnitID = 5
update dbo.VolumeUnit set VolumeUnitPluralizedName = 'Million Gallons', VolumeUnitDisplayName = 'Million Gallon', VolumeUnitName ='GallonsInMillions' where VolumeUnitID = 6

