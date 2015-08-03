# 반다이 남코 현역 프로그래머가 알려주는 유니티 네트워크 프로그래밍

## (1) 원서의 예제소스
원 출판사인 소프트뱅크 크리에이티브 사 홈페이지에서 내려받을 수 있습니다. 
주소창에 http://bit.ly/1MR8ekM를 입력하면 바로 파일이 전송됩니다. 
이 책의 예제는 유니티 4.5.1로 작성되었습니다. 
유니티 버전이 다르거나 컴퓨터 네트워크 환경에 따라 실습 진행이 어려울 수 있으니 
가급적 같은 버전을 내려받아 사용하기 바랍니다.

## (2) 번역된 예제소스
길벗 IT전문서 깃허브 저장소 https://github.com/gilbutITbook/006772에서 내려받을 수 있습니다.
이 책의 예제는 유니티 4.5.1로 작성되었습니다. 
유니티 버전이 다르거나 컴퓨터 네트워크 환경에 따라 실습 진행에 어려움이 있을 수 있으니
가급적 같은 버전을 내려받아 사용하시기 바랍니다.

## (3) 실행 파일이 이미 빌드되어 있습니다.
각 장의 bin 폴더에 있는 exe 파일은 이미 빌드된 실행 파일입니다. 
이 파일을 실행하여 게임을 해볼 수 있습니다. 
실행 과정 동영상(https://youtu.be/W7VNGctZ8yA)을 제공합니다. 

## (4) 유니티 4.5.1에서 빌드
유니티 4.5.1에서 게임을 빌드하고 실행할 수 있습니다. 
이때 Project가 아니라 Scene 파일을 열어서 빌드합니다. 
Scene 파일은 각 장의 Assets 폴더(> Scenes 폴더)의  Unity scene file입니다. 

## (5) MonoDevelop 설정 바꾸기
유니티 4.5.1이나 상위 버전에서 게임이 제대로 실행되지 않을 때는 
MonoDevelop의 설정을 'Mono / .NET 3.5'에서 'Mono / .NET 4.5'로 바꿔봅니다. 
MonoDevelop의 Solution 창에서 Assembly-CSharp를 선택하고 [우클릭]-[Options]를 선택합니다.
Project-Options의 [Build]-[General]-[Target framework] 항목을 'Mono / .NET 4.5'로 선택하고 OK를 누르세요.
'읽어주세요'와 같은 폴더 안에 ‘MonoDevelop 'Mono / .NET 4.5'로 바꾸는 과정을 설명한 파워포인트를 제공합니다. 

## (6) 유니티 5.1에서 빌드
8, 9장은 유니티 5.1에서 실습을 진행하는 데 어려움이 있음을 확인했습니다. 
그 원인은 유니티 4 버전에서 사용하던 명령어를 유니티 5 버전에서는 사용하지 않기 때문입니다. 
오류가 나는 명령어를 수정해야 올바르게 동작합니다.
저자에게 수정을 요청하였으며 수정본을 기다리는 중입니다. 
임시로 수정한 예제소스를 'Unity 5.1 예제' 폴더에서 제공합니다.

## (7) 'Unity 5.1 예제’ 폴더의 9장
'Unity 5.1 예제’ 폴더의 9장에서 분홍색으로 표시되는 부분이 있습니다. 
유니티 5.1 버전으로 업데이트할 때 Materials이 제대로 변환되지 않기 때문입니다.
변환되지 않은 Materials이 게임에서 분홍색으로 표시됩니다. 
저자에게 수정을 요청하였으며 수정본을 기다리는 중입니다. 

## (8) 게임에서 분홍색으로 표시되는 Materials
09\dokidoki_ganmodoki\Assets\Item\Material\ItemMaterial_ice00.mat
09\dokidoki_ganmodoki\Assets\Material\Cake Timer Material.mat
09\dokidoki_ganmodoki\Assets\Material\Sprite Ice Material.mat
09\dokidoki_ganmodoki\Assets\Models\Materials\M35_cow.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Daizuya\daizuya_anim_ganmo_00.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Character\Daizuya\Materials\daizuya_body_parts_col.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Daizuya\Materials\daizuya_hair_col.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Daizuya\Materials\daizuya_happi_col.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Irimameya\irimameya_anim_ganmo_00.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Character\Irimameya\Materials\irimameya_body_parts_col.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Irimameya\Materials\irimameya_hair_col.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Irimameya\Materials\irimameya_happi_col.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Toufuya\toufuya_anim_ganmo_00.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Character\Toufuya\toufuya_anim_out.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Character\Toufuya\toufuya_anim_out02.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Character\Toufuya\Materials\tofuya_body.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Toufuya\Materials\tofuya_face.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Toufuya\Materials\tofuya_hair.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Zundaya\zundaya_anim_ganmo_00.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Character\Zundaya\Materials\zundaya_body.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Zundaya\Materials\zundaya_face.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Zundaya\Materials\zundaya_hair.mat
09\dokidoki_ganmodoki\Assets\NewModels\Character\Zundaya\Materials\zundaya_parts.mat
09\dokidoki_ganmodoki\Assets\NewModels\Etc\SOWET01.fbx
09\dokidoki_ganmodoki\Assets\NewModels\Etc\Materials\sowet01.mat
09\dokidoki_ganmodoki\Assets\NewModels\Etc\Materials\sowet02.mat
09\dokidoki_ganmodoki\Assets\NewModels\Room\InnerWallTransparency.mat
