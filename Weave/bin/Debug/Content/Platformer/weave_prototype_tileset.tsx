<?xml version="1.0" encoding="UTF-8"?>
<tileset version="1.2" tiledversion="1.3.2" name="weave_prototype_tileset" tilewidth="32" tileheight="32" tilecount="40" columns="20">
 <image source="ruins_tile_set.png" width="640" height="64"/>
 <tile id="1">
  <objectgroup draworder="index" id="4">
   <object id="6" x="16.9235" y="18.8762" width="0.0813628"/>
   <object id="7" x="16.3539" y="15.2148" width="0.0813628"/>
   <object id="8" x="0.25797" y="-0.0345983" width="31.8213" height="31.8838"/>
  </objectgroup>
 </tile>
 <tile id="11">
  <properties>
   <property name="nez:isSlope" type="bool" value="true"/>
   <property name="nez:slopeTopLeft" type="int" value="0"/>
   <property name="nez:slopeTopRight" type="int" value="31"/>
  </properties>
  <objectgroup draworder="index">
   <object id="1" x="0" y="0">
    <polygon points="0,0 0,16 16,16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="12">
  <properties>
   <property name="nez:isSlope" type="bool" value="true"/>
   <property name="nez:slopeTopLeft" type="int" value="31"/>
   <property name="nez:slopeTopRight" type="int" value="0"/>
  </properties>
  <objectgroup draworder="index">
   <object id="1" x="0" y="0">
    <polygon points="0,0 0,16 16,16"/>
   </object>
  </objectgroup>
 </tile>
 <tile id="21">
    <properties>
    <property name="filewtf" type="file" value=""/>
    <property name="floaty" type="float" value="0"/>
    <property name="inty" type="int" value="0"/>
    <property name="nez:isOneWayPlatform" type="bool" value="true"/>
    <property name="poop" type="color" value=""/>
    </properties>
    <objectgroup draworder="index">
    <object id="1" x="0" y="0" width="32" height="3"/>
    </objectgroup>
 </tile>
</tileset>
